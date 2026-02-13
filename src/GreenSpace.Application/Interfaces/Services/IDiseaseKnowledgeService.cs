using GreenSpace.Application.DTOs.Diagnosis;

namespace GreenSpace.Application.Interfaces.Services;

/// <summary>
/// Service interface for disease knowledge base matching
/// </summary>
public interface IDiseaseKnowledgeService
{
    /// <summary>
    /// Find matching diseases from knowledge base based on symptoms
    /// </summary>
    /// <param name="description">User's description of symptoms</param>
    /// <param name="plantType">Optional plant type for filtering</param>
    /// <returns>Best matching disease if score >= threshold, null otherwise</returns>
    Task<DiseaseMatchResult?> FindMatchingDiseaseAsync(string description, string? plantType);

    /// <summary>
    /// Get all plant types for dropdown/selection
    /// </summary>
    Task<List<PlantTypeDto>> GetAllPlantTypesAsync();

    /// <summary>
    /// Get all diseases (for admin)
    /// </summary>
    Task<List<DiseaseDto>> GetAllDiseasesAsync();
}

/// <summary>
/// Result of disease matching from knowledge base
/// </summary>
public class DiseaseMatchResult
{
    /// <summary>
    /// Disease ID
    /// </summary>
    public Guid DiseaseId { get; set; }

    /// <summary>
    /// Match score (0.0 - 1.0)
    /// </summary>
    public double MatchScore { get; set; }

    /// <summary>
    /// The diagnosis response built from KB
    /// </summary>
    public DiagnosisResponseDto DiagnosisResponse { get; set; } = null!;

    /// <summary>
    /// List of matched symptoms
    /// </summary>
    public List<string> MatchedSymptoms { get; set; } = new();

    /// <summary>
    /// Total symptoms of the disease
    /// </summary>
    public int TotalSymptoms { get; set; }

    /// <summary>
    /// Product keywords for recommendations (from Disease.ProductKeywords)
    /// </summary>
    public List<string> ProductKeywords { get; set; } = new();
}

/// <summary>
/// DTO for plant type
/// </summary>
public class PlantTypeDto
{
    public Guid Id { get; set; }
    public string CommonName { get; set; } = string.Empty;
    public string? ScientificName { get; set; }
    public string? Family { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>
/// DTO for disease (admin view)
/// </summary>
public class DiseaseDto
{
    public Guid Id { get; set; }
    public string DiseaseName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public string? Description { get; set; }
    public string Severity { get; set; } = "Medium";
    public List<string> Causes { get; set; } = new();
    public List<string> Symptoms { get; set; } = new();
    public int SymptomCount { get; set; }
}
