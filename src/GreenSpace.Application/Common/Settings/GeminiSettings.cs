namespace GreenSpace.Application.Common.Settings;

/// <summary>
/// Settings for Google Gemini AI API
/// </summary>
public class GeminiSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Gemini";

    /// <summary>
    /// Gemini API Key
    /// Get from: https://aistudio.google.com/app/apikey
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model to use (default: gemini-1.5-flash)
    /// Options: gemini-1.5-flash, gemini-1.5-pro, gemini-pro-vision
    /// </summary>
    public string Model { get; set; } = "gemini-1.5-flash";

    /// <summary>
    /// Base URL for Gemini API
    /// </summary>
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";

    /// <summary>
    /// Maximum tokens in response
    /// </summary>
    public int MaxOutputTokens { get; set; } = 2048;

    /// <summary>
    /// Temperature for response randomness (0.0 - 1.0)
    /// Lower = more deterministic
    /// </summary>
    public double Temperature { get; set; } = 0.4;

    /// <summary>
    /// Whether Gemini feature is enabled
    /// If false, diagnosis endpoints will return service unavailable
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
