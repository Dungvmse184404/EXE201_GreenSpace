using GreenSpace.Application.DTOs.Diagnosis;

namespace GreenSpace.Application.Interfaces.External;

/// <summary>
/// Interface for AI Vision service (Gemini, Groq, OpenAI, etc.)
/// Used for plant disease diagnosis
/// </summary>
public interface IAIVisionService
{
    /// <summary>
    /// Check if AI service is available and properly configured
    /// </summary>
    /// <returns>True if service is ready to use</returns>
    bool IsAvailable();

    /// <summary>
    /// Get current model name
    /// </summary>
    string GetModelName();

    /// <summary>
    /// Get provider name (Gemini, Groq, OpenAI, etc.)
    /// </summary>
    string GetProviderName();

    /// <summary>
    /// Analyze plant image and return diagnosis
    /// </summary>
    /// <param name="imageBase64">Base64 encoded image</param>
    /// <param name="imageUrl">URL of image (alternative to base64)</param>
    /// <param name="userDescription">Optional user description of symptoms</param>
    /// <param name="language">Response language (default: vi)</param>
    /// <returns>Result with raw JSON response and debug info</returns>
    Task<AIAnalysisResult> AnalyzePlantImageAsync(
        string? imageBase64,
        string? imageUrl,
        string? userDescription,
        string language = "vi");
}

/// <summary>
/// Result from AI API call
/// </summary>
public class AIAnalysisResult
{
    /// <summary>
    /// Whether the API call was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Raw JSON response content (if successful)
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Debug information
    /// </summary>
    public AIDebugInfo DebugInfo { get; set; } = new();
}

/// <summary>
/// Debug information from AI API
/// </summary>
public class AIDebugInfo
{
    /// <summary>
    /// AI Provider name (Gemini, Groq, OpenAI)
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// HTTP status code from AI API
    /// </summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>
    /// Error code from AI API
    /// </summary>
    public int? ErrorCode { get; set; }

    /// <summary>
    /// Error message from AI API
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Raw response from AI API (truncated)
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
    /// Error source: "AI" or "App"
    /// </summary>
    public string? ErrorSource { get; set; }
}
