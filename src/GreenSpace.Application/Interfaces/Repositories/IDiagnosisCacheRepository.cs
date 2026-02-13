using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Interfaces.Repositories;

/// <summary>
/// Repository for diagnosis cache operations
/// </summary>
public interface IDiagnosisCacheRepository : IGenericRepository<DiagnosisCache>
{
    /// <summary>
    /// Search for matching diagnoses using trigram similarity
    /// </summary>
    /// <param name="normalizedDescription">Normalized user description</param>
    /// <param name="symptoms">Extracted symptoms</param>
    /// <param name="plantType">Optional plant type filter</param>
    /// <param name="trigramThreshold">Minimum trigram similarity (0-1)</param>
    /// <param name="limit">Maximum results to return</param>
    Task<List<DiagnosisCacheCandidate>> SearchCandidatesAsync(
        string normalizedDescription,
        List<string> symptoms,
        string? plantType,
        double trigramThreshold = 0.3,
        int limit = 20);

    /// <summary>
    /// Increment hit count for a cache entry
    /// </summary>
    Task IncrementHitCountAsync(Guid cacheId);

    /// <summary>
    /// Get active cache entries that haven't expired
    /// </summary>
    Task<List<DiagnosisCache>> GetActiveCacheEntriesAsync();

    /// <summary>
    /// Clean up expired cache entries
    /// </summary>
    Task<int> CleanupExpiredEntriesAsync();
}

/// <summary>
/// Candidate for cache matching with scores
/// </summary>
public class DiagnosisCacheCandidate
{
    /// <summary>
    /// The cache entry ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Plant type from cache
    /// </summary>
    public string? PlantType { get; set; }

    /// <summary>
    /// Normalized description from cache
    /// </summary>
    public string NormalizedDescription { get; set; } = string.Empty;

    /// <summary>
    /// Symptoms extracted from cache
    /// </summary>
    public List<string> Symptoms { get; set; } = new();

    /// <summary>
    /// Disease name from cache
    /// </summary>
    public string DiseaseName { get; set; } = string.Empty;

    /// <summary>
    /// AI response JSON from cache
    /// </summary>
    public string AiResponse { get; set; } = string.Empty;

    /// <summary>
    /// Trigram similarity score (0-1)
    /// </summary>
    public double TrigramScore { get; set; }

    /// <summary>
    /// Number of times this cache entry has been used
    /// </summary>
    public int HitCount { get; set; }

    /// <summary>
    /// Number of matched symptoms
    /// </summary>
    public int MatchedSymptomCount { get; set; }

    /// <summary>
    /// List of matched symptoms
    /// </summary>
    public List<string> MatchedSymptoms { get; set; } = new();

    /// <summary>
    /// Final calculated score
    /// </summary>
    public double FinalScore { get; set; }
}
