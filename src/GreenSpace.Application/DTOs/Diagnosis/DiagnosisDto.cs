using System.ComponentModel.DataAnnotations;

namespace GreenSpace.Application.DTOs.Diagnosis;

// ============================================
// REQUEST DTOs
// ============================================

/// <summary>
/// Request DTO for plant diagnosis
/// </summary>
public class DiagnosisRequestDto
{
    /// <summary>
    /// Base64 encoded image data (optional if Description is provided)
    /// Supported formats: JPG, PNG, WEBP
    /// Max size: 4MB
    /// </summary>
    /// <example>data:image/jpeg;base64,/9j/4AAQSkZJRg...</example>
    public string? ImageBase64 { get; set; }

    /// <summary>
    /// URL of the image to analyze (optional if Description is provided)
    /// Must be publicly accessible
    /// </summary>
    /// <example>https://example.com/plant-image.jpg</example>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Description of plant symptoms/problems (required if no image provided).
    /// Can be used alone for text-only diagnosis or with image for more accurate results.
    /// Include: plant name, symptoms, environment conditions, care history.
    /// </summary>
    /// <example>Cay trau ba cua toi bi vang la, da trong trong nha 6 thang, tuoi nuoc 2 lan/tuan</example>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Preferred language for response (default: vi)
    /// </summary>
    /// <example>vi</example>
    [StringLength(5)]
    public string Language { get; set; } = "vi";

    /// <summary>
    /// Type of plant (optional, helps improve cache matching)
    /// </summary>
    /// <example>cây dừa</example>
    [StringLength(100)]
    public string? PlantType { get; set; }

    /// <summary>
    /// Skip cache and force AI call (default: false)
    /// </summary>
    public bool SkipCache { get; set; } = false;
}

// ============================================
// RESPONSE DTOs
// ============================================

/// <summary>
/// Response DTO for plant diagnosis
/// </summary>
public class DiagnosisResponseDto
{
    /// <summary>
    /// Whether the diagnosis was successful
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Error message if diagnosis failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Debug info - Gemini API error details (only in development)
    /// </summary>
    public GeminiDebugInfo? DebugInfo { get; set; }

    /// <summary>
    /// Identified plant information
    /// </summary>
    public PlantInfoDto? PlantInfo { get; set; }

    /// <summary>
    /// Detected disease/problem information
    /// </summary>
    public DiseaseInfoDto? DiseaseInfo { get; set; }

    /// <summary>
    /// Treatment recommendations
    /// </summary>
    public TreatmentInfoDto? Treatment { get; set; }

    /// <summary>
    /// Recommended products from GreenSpace store
    /// </summary>
    public List<RecommendedProductDto>? RecommendedProducts { get; set; }

    /// <summary>
    /// Confidence score (0-100)
    /// </summary>
    public int ConfidenceScore { get; set; }

    /// <summary>
    /// Timestamp of diagnosis
    /// </summary>
    public DateTime DiagnosedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this response was from cache
    /// </summary>
    public bool FromCache { get; set; } = false;

    /// <summary>
    /// Whether this response was from knowledge base
    /// </summary>
    public bool FromKnowledgeBase { get; set; } = false;

    /// <summary>
    /// Knowledge base match score (0.0 - 1.0) if from KB
    /// </summary>
    public double? KnowledgeBaseMatchScore { get; set; }

    /// <summary>
    /// Cache match score (0.0 - 1.0) if from cache
    /// </summary>
    public double? CacheMatchScore { get; set; }

    /// <summary>
    /// Cache ID if from cache (for debugging)
    /// </summary>
    public Guid? CacheId { get; set; }
}

/// <summary>
/// Plant identification information
/// </summary>
public class PlantInfoDto
{
    /// <summary>
    /// Common name of the plant
    /// </summary>
    /// <example>Cay trau ba</example>
    public string CommonName { get; set; } = string.Empty;

    /// <summary>
    /// Scientific name of the plant
    /// </summary>
    /// <example>Epipremnum aureum</example>
    public string? ScientificName { get; set; }

    /// <summary>
    /// Plant family
    /// </summary>
    /// <example>Araceae</example>
    public string? Family { get; set; }

    /// <summary>
    /// Brief description of the plant
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Disease/problem information
/// </summary>
public class DiseaseInfoDto
{
    /// <summary>
    /// Whether plant has any disease/problem
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Name of the disease/problem
    /// </summary>
    /// <example>Benh dom la</example>
    public string? DiseaseName { get; set; }

    /// <summary>
    /// Severity level: None, Low, Medium, High, Critical
    /// </summary>
    /// <example>Medium</example>
    public string Severity { get; set; } = "None";

    /// <summary>
    /// Visible symptoms
    /// </summary>
    public List<string> Symptoms { get; set; } = new();

    /// <summary>
    /// Possible causes
    /// </summary>
    public List<string> Causes { get; set; } = new();

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Treatment recommendations
/// </summary>
public class TreatmentInfoDto
{
    /// <summary>
    /// Immediate actions to take
    /// </summary>
    public List<string> ImmediateActions { get; set; } = new();

    /// <summary>
    /// Long-term care instructions
    /// </summary>
    public List<string> LongTermCare { get; set; } = new();

    /// <summary>
    /// Prevention tips for future
    /// </summary>
    public List<string> PreventionTips { get; set; } = new();

    /// <summary>
    /// Watering advice
    /// </summary>
    public string? WateringAdvice { get; set; }

    /// <summary>
    /// Lighting advice
    /// </summary>
    public string? LightingAdvice { get; set; }

    /// <summary>
    /// Fertilizing advice
    /// </summary>
    public string? FertilizingAdvice { get; set; }
}

/// <summary>
/// Recommended product from GreenSpace
/// </summary>
public class RecommendedProductDto
{
    /// <summary>
    /// Product ID
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product thumbnail
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Why this product is recommended
    /// </summary>
    public string? RecommendationReason { get; set; }
}

/// <summary>
/// Debug information from Gemini API
/// </summary>
public class GeminiDebugInfo
{
    /// <summary>
    /// HTTP status code from Gemini API
    /// </summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>
    /// Error code from Gemini API (e.g., 429 = quota exceeded, 404 = model not found)
    /// </summary>
    public int? GeminiErrorCode { get; set; }

    /// <summary>
    /// Error message from Gemini API
    /// </summary>
    public string? GeminiErrorMessage { get; set; }

    /// <summary>
    /// Raw response from Gemini API (truncated)
    /// </summary>
    public string? RawResponse { get; set; }

    /// <summary>
    /// Model used for diagnosis
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Whether image was included in request
    /// </summary>
    public bool HasImage { get; set; }

    /// <summary>
    /// Error source: "Gemini" or "App"
    /// </summary>
    public string? ErrorSource { get; set; }
}
