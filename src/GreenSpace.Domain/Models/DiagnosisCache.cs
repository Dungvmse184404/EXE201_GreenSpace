using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSpace.Domain.Models;

/// <summary>
/// Cache for AI diagnosis results to reduce API calls
/// </summary>
[Table("diagnosis_cache")]
public class DiagnosisCache
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Optional plant type for more accurate matching
    /// </summary>
    [Column("plant_type")]
    [StringLength(100)]
    public string? PlantType { get; set; }

    /// <summary>
    /// Original user description (for reference)
    /// </summary>
    [Column("original_description")]
    [Required]
    public string OriginalDescription { get; set; } = null!;

    /// <summary>
    /// Normalized description for matching (lowercase, cleaned)
    /// </summary>
    [Column("normalized_description")]
    [Required]
    public string NormalizedDescription { get; set; } = null!;

    /// <summary>
    /// Extracted symptoms from description
    /// </summary>
    [Column("symptoms")]
    public List<string> Symptoms { get; set; } = new();

    /// <summary>
    /// Disease name from AI diagnosis
    /// </summary>
    [Column("disease_name")]
    [StringLength(255)]
    [Required]
    public string DiseaseName { get; set; } = null!;

    /// <summary>
    /// Full AI response as JSON
    /// </summary>
    [Column("ai_response", TypeName = "jsonb")]
    [Required]
    public string AiResponse { get; set; } = null!;

    /// <summary>
    /// Confidence score from AI (0-100)
    /// </summary>
    [Column("confidence_score")]
    public int? ConfidenceScore { get; set; }

    /// <summary>
    /// Number of times this cache entry was used
    /// </summary>
    [Column("hit_count")]
    public int HitCount { get; set; }

    /// <summary>
    /// When this cache entry was created
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this cache entry expires
    /// </summary>
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Cache TTL in days (default 90)
    /// </summary>
    [Column("cache_ttl_days")]
    public int CacheTtlDays { get; set; } = 90;

    /// <summary>
    /// Whether this cache entry is active
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether image was included in original request
    /// </summary>
    [Column("has_image")]
    public bool HasImage { get; set; }
}
