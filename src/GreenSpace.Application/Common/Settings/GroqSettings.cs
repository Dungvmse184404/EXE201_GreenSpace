namespace GreenSpace.Application.Common.Settings;

/// <summary>
/// Configuration settings for Groq AI API
/// </summary>
public class GroqSettings
{
    public const string SectionName = "Groq";

    /// <summary>
    /// Groq API Key from https://console.groq.com/keys
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model to use (e.g., llama-3.2-90b-vision-preview, llama-3.2-11b-vision-preview)
    /// </summary>
    public string Model { get; set; } = "llama-3.2-11b-vision-preview";

    /// <summary>
    /// Groq API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1";

    /// <summary>
    /// Maximum tokens in response
    /// </summary>
    public int MaxTokens { get; set; } = 2048;

    /// <summary>
    /// Temperature for response generation (0-2)
    /// </summary>
    public double Temperature { get; set; } = 0.4;

    /// <summary>
    /// Enable/disable Groq service
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
