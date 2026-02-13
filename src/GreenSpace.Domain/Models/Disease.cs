using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSpace.Domain.Models;

/// <summary>
/// Entity for plant diseases (Knowledge Base)
/// </summary>
[Table("diseases")]
public class Disease
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Vietnamese disease name
    /// </summary>
    [Column("disease_name")]
    [Required]
    [MaxLength(200)]
    public string DiseaseName { get; set; } = string.Empty;

    /// <summary>
    /// English disease name
    /// </summary>
    [Column("english_name")]
    [MaxLength(200)]
    public string? EnglishName { get; set; }

    /// <summary>
    /// Detailed description of the disease
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Severity level: None, Low, Medium, High, Critical
    /// </summary>
    [Column("severity")]
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium";

    /// <summary>
    /// Causes of the disease
    /// </summary>
    [Column("causes", TypeName = "text[]")]
    public List<string> Causes { get; set; } = new();

    /// <summary>
    /// Immediate treatment actions
    /// </summary>
    [Column("immediate_actions", TypeName = "text[]")]
    public List<string> ImmediateActions { get; set; } = new();

    /// <summary>
    /// Long-term care instructions
    /// </summary>
    [Column("long_term_care", TypeName = "text[]")]
    public List<string> LongTermCare { get; set; } = new();

    /// <summary>
    /// Prevention tips
    /// </summary>
    [Column("prevention_tips", TypeName = "text[]")]
    public List<string> PreventionTips { get; set; } = new();

    /// <summary>
    /// Watering advice
    /// </summary>
    [Column("watering_advice")]
    [MaxLength(500)]
    public string? WateringAdvice { get; set; }

    /// <summary>
    /// Lighting advice
    /// </summary>
    [Column("lighting_advice")]
    [MaxLength(500)]
    public string? LightingAdvice { get; set; }

    /// <summary>
    /// Fertilizing advice
    /// </summary>
    [Column("fertilizing_advice")]
    [MaxLength(500)]
    public string? FertilizingAdvice { get; set; }

    /// <summary>
    /// Sample image URLs of the disease
    /// </summary>
    [Column("image_urls", TypeName = "text[]")]
    public List<string> ImageUrls { get; set; } = new();

    /// <summary>
    /// Product keywords for recommendations
    /// </summary>
    [Column("product_keywords", TypeName = "text[]")]
    public List<string> ProductKeywords { get; set; } = new();

    /// <summary>
    /// Additional notes
    /// </summary>
    [Column("notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Whether this disease is active
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Created timestamp
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<DiseaseSymptom> DiseaseSymptoms { get; set; } = new List<DiseaseSymptom>();
    public virtual ICollection<PlantTypeDisease> PlantTypeDiseases { get; set; } = new List<PlantTypeDisease>();
}
