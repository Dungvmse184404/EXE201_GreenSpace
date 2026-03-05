using GreenSpace.Application.DTOs.Chatbox;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Services;

/// <summary>
/// Service for AI-powered chatbox interactions about plants
/// Handles general Q&A, diagnosis, care advice, and product recommendations
/// </summary>
public interface IChatboxService
{
    /// <summary>
    /// Check if chatbox AI service is available
    /// </summary>
    /// <returns>True if service is ready</returns>
    bool IsAvailable();

    /// <summary>
    /// Send a message to the chatbox AI and get a response
    /// Supports text-only and image+text conversations
    /// </summary>
    /// <param name="request">Chatbox request with user message and optional image</param>
    /// <param name="userId">Optional user ID for conversation history tracking</param>
    /// <returns>AI response with message, recommendations, and optional diagnosis</returns>
    Task<IServiceResult<ChatboxResponseDto>> SendMessageAsync(ChatboxRequestDto request, Guid? userId = null);

    /// <summary>
    /// Get service status and model information
    /// </summary>
    /// <returns>Service status details</returns>
    Task<ServiceStatusDto> GetServiceStatusAsync();
}

/// <summary>
/// Service status information
/// </summary>
public class ServiceStatusDto
{
    /// <summary>
    /// Whether service is available
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// AI provider name (Groq, Gemini, etc.)
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// AI model name
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Status message
    /// </summary>
    public string? Message { get; set; }
}
