namespace GreenSpace.Application.Interfaces.External;

/// <summary>
/// Interface for simple chat completions (conversational AI)
/// Used for chatbot-like interactions (not structured diagnosis)
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Check if chat service is available
    /// </summary>
    bool IsAvailable();

    /// <summary>
    /// Get provider name (Groq, Gemini, etc.)
    /// </summary>
    string GetProviderName();

    /// <summary>
    /// Get model name
    /// </summary>
    string GetModelName();

    /// <summary>
    /// Send a chat message and get conversational response (like ChatGPT)
    /// </summary>
    /// <param name="message">User message</param>
    /// <param name="language">Response language (vi or en)</param>
    /// <returns>Natural conversational response</returns>
    Task<ChatCompletionResult> GetChatResponseAsync(string message, string language = "vi");
}

/// <summary>
/// Result from chat API
/// </summary>
public class ChatCompletionResult
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Conversational response from AI
    /// </summary>
    public string? Response { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Debug info
    /// </summary>
    public ChatDebugInfo DebugInfo { get; set; } = new();
}

/// <summary>
/// Debug info for chat completion
/// </summary>
public class ChatDebugInfo
{
    public string? Provider { get; set; }
    public string? Model { get; set; }
    public int? HttpStatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
}
