using System.Text.Json;
using GreenSpace.Application.DTOs.Diagnosis;
using GreenSpace.Application.Helpers;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Models;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services;

/// <summary>
/// Service for diagnosis caching with semantic matching
/// Uses hybrid matching: trigram similarity + symptom overlap + token overlap
/// </summary>
public class DiagnosisCacheService : IDiagnosisCacheService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DiagnosisCacheService> _logger;
    private readonly PlantPartSymptomExtractor _symptomExtractor;

    // Matching thresholds and weights
    private const double MatchThreshold = 0.6;
    private const double TrigramWeight = 0.4;
    private const double SymptomWeight = 0.4;
    private const double TokenWeight = 0.2;
    private const double PlantTypeBonus = 0.1;

    // Cache for symptom dictionary (loaded once per request scope)
    private List<SymptomDictionary>? _symptomDictionary;

    public DiagnosisCacheService(
        IUnitOfWork unitOfWork,
        ILogger<DiagnosisCacheService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _symptomExtractor = new PlantPartSymptomExtractor();
    }

    /// <inheritdoc/>
    public async Task<DiagnosisCacheMatchResult?> FindMatchAsync(
        string description,
        string? plantType,
        bool hasImage = false)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        try
        {
            // 1. Normalize the input description
            var normalizedDescription = VietnameseTextHelper.Normalize(description);
            _logger.LogDebug("Normalized description: {Normalized}", normalizedDescription);

            // 2. Extract symptoms from description
            var symptoms = await ExtractSymptomsAsync(description);
            _logger.LogDebug("Extracted symptoms: {Symptoms}", string.Join(", ", symptoms));

            // 3. Search for candidates using trigram similarity
            var candidates = await _unitOfWork.DiagnosisCacheRepository
                .SearchCandidatesAsync(normalizedDescription, symptoms, plantType, 0.3, 20);

            if (candidates.Count == 0)
            {
                _logger.LogDebug("No cache candidates found");
                return null;
            }

            _logger.LogDebug("Found {Count} cache candidates", candidates.Count);

            // 4. Score each candidate
            var scoredCandidates = new List<(DiagnosisCacheCandidate Candidate, double Score, MatchScoreBreakdown Breakdown)>();

            foreach (var candidate in candidates)
            {
                var breakdown = new MatchScoreBreakdown();

                // Trigram score (already from database)
                breakdown.TrigramScore = candidate.TrigramScore;

                // Symptom Jaccard score
                if (symptoms.Count > 0 || candidate.Symptoms.Count > 0)
                {
                    breakdown.SymptomScore = VietnameseTextHelper.CalculateJaccardSimilarity(
                        symptoms, candidate.Symptoms);
                }
                else
                {
                    breakdown.SymptomScore = 0;
                }

                // Token overlap score
                breakdown.TokenScore = VietnameseTextHelper.CalculateTokenOverlap(
                    normalizedDescription, candidate.NormalizedDescription);

                // Calculate weighted score
                double score = (breakdown.TrigramScore * TrigramWeight) +
                               (breakdown.SymptomScore * SymptomWeight) +
                               (breakdown.TokenScore * TokenWeight);

                // Plant type bonus
                if (!string.IsNullOrWhiteSpace(plantType) &&
                    !string.IsNullOrWhiteSpace(candidate.PlantType) &&
                    VietnameseTextHelper.Normalize(plantType) == VietnameseTextHelper.Normalize(candidate.PlantType))
                {
                    breakdown.PlantTypeMatched = true;
                    breakdown.PlantTypeBonus = PlantTypeBonus;
                    score += PlantTypeBonus;
                }

                scoredCandidates.Add((candidate, score, breakdown));

                _logger.LogDebug(
                    "Candidate {Id}: Trigram={Trigram:F2}, Symptom={Symptom:F2}, Token={Token:F2}, Total={Total:F2}",
                    candidate.Id, breakdown.TrigramScore, breakdown.SymptomScore, breakdown.TokenScore, score);
            }

            // 5. Find best match
            var bestMatch = scoredCandidates
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            if (bestMatch.Score < MatchThreshold)
            {
                _logger.LogDebug("Best match score {Score:F2} below threshold {Threshold}",
                    bestMatch.Score, MatchThreshold);
                return null;
            }

            _logger.LogInformation(
                "Cache hit! Disease: {Disease}, Score: {Score:F2}",
                bestMatch.Candidate.DiseaseName, bestMatch.Score);

            // 6. Parse cached AI response
            var diagnosisResponse = ParseCachedResponse(bestMatch.Candidate.AiResponse);
            if (diagnosisResponse == null)
            {
                _logger.LogWarning("Failed to parse cached response for {Id}", bestMatch.Candidate.Id);
                return null;
            }

            return new DiagnosisCacheMatchResult
            {
                CacheId = bestMatch.Candidate.Id,
                DiagnosisResponse = diagnosisResponse,
                MatchScore = bestMatch.Score,
                ScoreBreakdown = bestMatch.Breakdown,
                DiseaseName = bestMatch.Candidate.DiseaseName,
                HitCount = bestMatch.Candidate.HitCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding cache match for description");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task SaveToCacheAsync(
        string description,
        string? plantType,
        DiagnosisResponseDto diagnosisResponse,
        bool hasImage = false,
        int cacheTtlDays = 90)
    {
        if (string.IsNullOrWhiteSpace(description) || diagnosisResponse?.DiseaseInfo == null)
            return;

        try
        {
            var normalizedDescription = VietnameseTextHelper.Normalize(description);
            var symptoms = await ExtractSymptomsAsync(description);

            var cacheEntry = new DiagnosisCache
            {
                Id = Guid.NewGuid(),
                PlantType = plantType,
                OriginalDescription = description,
                NormalizedDescription = normalizedDescription,
                Symptoms = symptoms,
                DiseaseName = diagnosisResponse.DiseaseInfo.DiseaseName ?? "Unknown",
                AiResponse = JsonSerializer.Serialize(diagnosisResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                }),
                ConfidenceScore = diagnosisResponse.ConfidenceScore,
                HitCount = 0,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(cacheTtlDays),
                CacheTtlDays = cacheTtlDays,
                IsActive = true,
                HasImage = hasImage
            };

            await _unitOfWork.DiagnosisCacheRepository.AddAsync(cacheEntry);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Saved diagnosis to cache: Disease={Disease}, TTL={TTL} days",
                cacheEntry.DiseaseName, cacheTtlDays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving diagnosis to cache");
            // Don't throw - cache save failure shouldn't break the main flow
        }
    }

    /// <inheritdoc/>
    public async Task IncrementHitCountAsync(Guid cacheId)
    {
        try
        {
            await _unitOfWork.DiagnosisCacheRepository.IncrementHitCountAsync(cacheId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing hit count for cache {CacheId}", cacheId);
        }
    }

    /// <inheritdoc/>
    public async Task<int> CleanupExpiredEntriesAsync()
    {
        try
        {
            var count = await _unitOfWork.DiagnosisCacheRepository.CleanupExpiredEntriesAsync();
            if (count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired cache entries", count);
            }
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired cache entries");
            return 0;
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> ExtractSymptomsAsync(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return new List<string>();

        // Load symptom dictionary if not cached
        _symptomDictionary ??= await _unitOfWork.SymptomDictionaryRepository.GetAllSymptomsAsync();

        // Use scope-based extraction for more accurate symptom identification
        var scopedSymptoms = _symptomExtractor.ExtractWithScope(description, _symptomDictionary);

        // Flatten to list of symptom names (for backward compatibility with cache)
        var extractedSymptoms = _symptomExtractor.FlattenSymptoms(scopedSymptoms);

        // Also extract using pattern matching for symptoms not in dictionary
        var patternSymptoms = VietnameseTextHelper.ExtractSymptomPhrases(description);
        foreach (var phrase in patternSymptoms)
        {
            var normalized = VietnameseTextHelper.Normalize(phrase);
            if (!extractedSymptoms.Any(s => VietnameseTextHelper.Normalize(s) == normalized))
            {
                extractedSymptoms.Add(phrase);
            }
        }

        return extractedSymptoms.ToList();
    }

    /// <summary>
    /// Extract symptoms with plant part context (scope-based)
    /// Returns symptoms grouped by plant part for more accurate matching
    /// </summary>
    public async Task<Dictionary<string, List<ExtractedSymptom>>> ExtractSymptomsWithScopeAsync(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return new Dictionary<string, List<ExtractedSymptom>>();

        _symptomDictionary ??= await _unitOfWork.SymptomDictionaryRepository.GetAllSymptomsAsync();
        return _symptomExtractor.ExtractWithScope(description, _symptomDictionary);
    }

    /// <summary>
    /// Parse cached JSON response to DiagnosisResponseDto
    /// </summary>
    private DiagnosisResponseDto? ParseCachedResponse(string jsonResponse)
    {
        try
        {
            return JsonSerializer.Deserialize<DiagnosisResponseDto>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing cached response JSON");
            return null;
        }
    }
}
