using GreenSpace.Application.DTOs.Diagnosis;

namespace GreenSpace.Application.Interfaces.Services;

/// <summary>
/// Service interface for diagnosis caching with semantic matching
/// </summary>
public interface IDiagnosisCacheService
{
    /// <summary>
    /// Find a cached diagnosis that matches the given description
    /// Uses hybrid matching: trigram similarity + symptom overlap + token overlap
    /// </summary>
    /// <param name="description">User's description of plant symptoms</param>
    /// <param name="plantType">Optional plant type for better matching</param>
    /// <param name="hasImage">Whether the request includes an image</param>
    /// <returns>Cached diagnosis if match score >= threshold, null otherwise</returns>
    Task<DiagnosisCacheMatchResult?> FindMatchAsync(string description, string? plantType, bool hasImage = false);

    /// <summary>
    /// Save a new diagnosis result to cache
    /// </summary>
    /// <param name="description">Original user description</param>
    /// <param name="plantType">Optional plant type</param>
    /// <param name="diagnosisResponse">The AI diagnosis response</param>
    /// <param name="hasImage">Whether the diagnosis included an image</param>
    /// <param name="cacheTtlDays">Cache TTL in days (default 90)</param>
    Task SaveToCacheAsync(
        string description,
        string? plantType,
        DiagnosisResponseDto diagnosisResponse,
        bool hasImage = false,
        int cacheTtlDays = 90);

    /// <summary>
    /// Increment hit count for a cached entry
    /// </summary>
    Task IncrementHitCountAsync(Guid cacheId);

    /// <summary>
    /// Clean up expired cache entries
    /// </summary>
    /// <returns>Number of entries removed</returns>
    Task<int> CleanupExpiredEntriesAsync();

    /// <summary>
    /// Extract symptoms from description using the symptom dictionary
    /// </summary>
    /// <param name="description">User description</param>
    /// <returns>List of matched canonical symptom names</returns>
    Task<List<string>> ExtractSymptomsAsync(string description);
}

/// <summary>
/// Result of a cache match lookup
/// </summary>
public class DiagnosisCacheMatchResult
{
    /// <summary>
    /// The cached diagnosis ID
    /// </summary>
    public Guid CacheId { get; set; }

    /// <summary>
    /// The cached diagnosis response
    /// </summary>
    public DiagnosisResponseDto DiagnosisResponse { get; set; } = null!;

    /// <summary>
    /// The final match score (0.0 - 1.0)
    /// </summary>
    public double MatchScore { get; set; }

    /// <summary>
    /// Breakdown of the match score
    /// </summary>
    public MatchScoreBreakdown ScoreBreakdown { get; set; } = new();

    /// <summary>
    /// The disease name from cache
    /// </summary>
    public string DiseaseName { get; set; } = string.Empty;

    /// <summary>
    /// Number of times this cache entry has been used
    /// </summary>
    public int HitCount { get; set; }
}

/// <summary>
/// Breakdown of match score components
/// </summary>
public class MatchScoreBreakdown
{
    /// <summary>
    /// Trigram similarity score (0-1)
    /// </summary>
    public double TrigramScore { get; set; }

    /// <summary>
    /// Symptom Jaccard similarity (0-1)
    /// </summary>
    public double SymptomScore { get; set; }

    /// <summary>
    /// Token overlap score (0-1)
    /// </summary>
    public double TokenScore { get; set; }

    /// <summary>
    /// Whether plant type matched
    /// </summary>
    public bool PlantTypeMatched { get; set; }

    /// <summary>
    /// Plant type bonus applied
    /// </summary>
    public double PlantTypeBonus { get; set; }
}
