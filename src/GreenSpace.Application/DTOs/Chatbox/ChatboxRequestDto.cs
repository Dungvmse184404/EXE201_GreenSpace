using System.ComponentModel.DataAnnotations;

namespace GreenSpace.Application.DTOs.Chatbox;

/// <summary>
/// Request DTO for chatbox AI interaction
/// </summary>
public class ChatboxRequestDto
{
    /// <summary>
    /// User's message/question about plants, care, products, etc.
    /// </summary>
    /// <example>How do I care for a monstera plant?</example>
    [Required(ErrorMessage = "Message is required")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 2000 characters")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional image base64 for visual analysis (e.g., uploading a plant photo)
    /// Supported formats: JPG, PNG, WEBP
    /// Max size: 4MB
    /// </summary>
    /// <example>data:image/jpeg;base64,/9j/4AAQSkZJRg...</example>
    public string? ImageBase64 { get; set; }

    /// <summary>
    /// Optional image URL for visual analysis
    /// Must be publicly accessible
    /// </summary>
    /// <example>https://example.com/plant-image.jpg</example>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Preferred language for response (default: vi)
    /// Supported: vi (Vietnamese), en (English)
    /// </summary>
    /// <example>vi</example>
    [StringLength(5)]
    public string Language { get; set; } = "vi";

    /// <summary>
    /// Type of plant (optional, helps improve context)
    /// </summary>
    /// <example>Monstera Deliciosa</example>
    [StringLength(100)]
    public string? PlantType { get; set; }

    /// <summary>
    /// Include product recommendations in response (default: true)
    /// </summary>
    public bool IncludeProductRecommendations { get; set; } = true;

    /// <summary>
    /// Skip cache and force AI call (default: false)
    /// </summary>
    public bool SkipCache { get; set; } = false;
}
