namespace GreenSpace.Application.DTOs.Chatbox;

/// <summary>
/// Response DTO for chatbox AI interaction
/// </summary>
public class ChatboxResponseDto
{
    /// <summary>
    /// Whether the response was successful
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// AI response message to user
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Type of response (general, diagnosis, recommendation, etc.)
    /// </summary>
    public string ResponseType { get; set; } = "general"; // general, diagnosis, care_advice, product_recommendation

    /// <summary>
    /// Detected plant information (if image was provided)
    /// </summary>
    public PlantDetectionDto? PlantDetection { get; set; }

    /// <summary>
    /// Detected disease/problem (if applicable)
    /// </summary>
    public DiseaseDetectionDto? DiseaseDetection { get; set; }

    /// <summary>
    /// Recommended products from GreenSpace store
    /// </summary>
    public List<ChatboxProductRecommendationDto>? RecommendedProducts { get; set; }

    /// <summary>
    /// Additional context or follow-up suggestions
    /// </summary>
    public List<string>? SuggestedFollowUps { get; set; }

    /// <summary>
    /// Debug info (only in development environment)
    /// </summary>
    public ChatboxDebugInfoDto? DebugInfo { get; set; }

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Plant detection information
/// </summary>
public class PlantDetectionDto
{
    /// <summary>
    /// Detected plant name
    /// </summary>
    public string? PlantName { get; set; }

    /// <summary>
    /// Scientific name if available
    /// </summary>
    public string? ScientificName { get; set; }

    /// <summary>
    /// Confidence level (0-100)
    /// </summary>
    public int ConfidenceLevel { get; set; }

    /// <summary>
    /// Plant characteristics
    /// </summary>
    public string? Characteristics { get; set; }
}

/// <summary>
/// Disease detection information
/// </summary>
public class DiseaseDetectionDto
{
    /// <summary>
    /// Disease name
    /// </summary>
    public string? DiseaseName { get; set; }

    /// <summary>
    /// Severity level (mild, moderate, severe)
    /// </summary>
    public string SeverityLevel { get; set; } = "unknown";

    /// <summary>
    /// Symptoms observed
    /// </summary>
    public List<string>? Symptoms { get; set; }

    /// <summary>
    /// Recommended treatment
    /// </summary>
    public string? Treatment { get; set; }
}

/// <summary>
/// Product recommendation from chatbox
/// </summary>
public class ChatboxProductRecommendationDto
{
    /// <summary>
    /// Product ID
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Why this product is recommended
    /// </summary>
    public string RecommendationReason { get; set; } = string.Empty;

    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Product category
    /// </summary>
    public string? Category { get; set; }
}

/// <summary>
/// Debug information for development
/// </summary>
public class ChatboxDebugInfoDto
{
    /// <summary>
    /// AI provider used (Groq, Gemini, etc.)
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// AI model name
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Whether image was analyzed
    /// </summary>
    public bool HasImage { get; set; }

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Cache hit indicator
    /// </summary>
    public bool CacheHit { get; set; }

    /// <summary>
    /// Any error from AI provider
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// HTTP status code from external service
    /// </summary>
    public int? HttpStatusCode { get; set; }
}
