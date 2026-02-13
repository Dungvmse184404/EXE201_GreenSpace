using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Domain.Models;
using GreenSpace.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for diagnosis cache with PostgreSQL trigram support
/// </summary>
public class DiagnosisCacheRepository : GenericRepository<DiagnosisCache>, IDiagnosisCacheRepository
{
    private readonly AppDbContext _context;

    public DiagnosisCacheRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<DiagnosisCacheCandidate>> SearchCandidatesAsync(
        string normalizedDescription,
        List<string> symptoms,
        string? plantType,
        double trigramThreshold = 0.3,
        int limit = 20)
    {
        var now = DateTime.UtcNow;

        // Base query - active and not expired
        var query = _context.DiagnosisCaches
            .Where(c => c.IsActive && c.ExpiresAt > now);

        // Filter by plant type if provided
        if (!string.IsNullOrWhiteSpace(plantType))
        {
            // Include entries matching plant type OR entries without plant type
            query = query.Where(c => c.PlantType == null ||
                                     c.PlantType == plantType ||
                                     EF.Functions.ILike(c.PlantType, $"%{plantType}%"));
        }

        // Use raw SQL for trigram similarity (pg_trgm)
        // Note: EF.Functions.TrigramsSimilarity requires Npgsql.EntityFrameworkCore.PostgreSQL.Trigrams
        var candidates = await _context.DiagnosisCaches
            .FromSqlRaw(@"
                SELECT dc.*
                FROM diagnosis_cache dc
                WHERE dc.is_active = true
                  AND dc.expires_at > {0}
                  AND similarity(dc.normalized_description, {1}) > {2}
                ORDER BY similarity(dc.normalized_description, {1}) DESC
                LIMIT {3}",
                now, normalizedDescription, trigramThreshold, limit)
            .AsNoTracking()
            .ToListAsync();

        // Calculate scores for each candidate
        var result = new List<DiagnosisCacheCandidate>();

        foreach (var cache in candidates)
        {
            var candidate = new DiagnosisCacheCandidate
            {
                Id = cache.Id,
                PlantType = cache.PlantType,
                NormalizedDescription = cache.NormalizedDescription,
                Symptoms = cache.Symptoms,
                DiseaseName = cache.DiseaseName,
                AiResponse = cache.AiResponse,
                HitCount = cache.HitCount,
                TrigramScore = await CalculateTrigramScoreAsync(cache.NormalizedDescription, normalizedDescription)
            };

            // Calculate symptom overlap
            if (symptoms.Any() && cache.Symptoms.Any())
            {
                var matchedSymptoms = symptoms.Intersect(cache.Symptoms, StringComparer.OrdinalIgnoreCase).ToList();
                candidate.MatchedSymptomCount = matchedSymptoms.Count;
                candidate.MatchedSymptoms = matchedSymptoms;
            }

            result.Add(candidate);
        }

        return result;
    }

    /// <summary>
    /// Calculate trigram similarity using PostgreSQL function
    /// </summary>
    private async Task<double> CalculateTrigramScoreAsync(string text1, string text2)
    {
        try
        {
            var result = await _context.Database
                .SqlQueryRaw<double>($"SELECT similarity({text1}, {text2})")
                .FirstOrDefaultAsync();
            return result;
        }
        catch
        {
            // Fallback to simple calculation if trigram not available
            return CalculateSimpleSimilarity(text1, text2);
        }
    }

    /// <summary>
    /// Simple similarity fallback
    /// </summary>
    private double CalculateSimpleSimilarity(string text1, string text2)
    {
        var tokens1 = text1.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var tokens2 = text2.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!tokens1.Any() || !tokens2.Any()) return 0;

        var intersection = tokens1.Intersect(tokens2).Count();
        var union = tokens1.Union(tokens2).Count();

        return (double)intersection / union;
    }

    /// <inheritdoc/>
    public async Task IncrementHitCountAsync(Guid cacheId)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "UPDATE diagnosis_cache SET hit_count = hit_count + 1 WHERE id = {0}",
            cacheId);
    }

    /// <inheritdoc/>
    public async Task<List<DiagnosisCache>> GetActiveCacheEntriesAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.DiagnosisCaches
            .Where(c => c.IsActive && c.ExpiresAt > now)
            .OrderByDescending(c => c.HitCount)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CleanupExpiredEntriesAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.DiagnosisCaches
            .Where(c => c.ExpiresAt <= now)
            .ExecuteDeleteAsync();
    }
}
