using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSpace.Domain.Models;

/// <summary>
/// Entity for plant types/species
/// </summary>
[Table("plant_types")]
public class PlantType
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Common name of the plant
    /// </summary>
    [Column("common_name")]
    [Required]
    [MaxLength(100)]
    public string CommonName { get; set; } = string.Empty;

    /// <summary>
    /// Scientific name
    /// </summary>
    [Column("scientific_name")]
    [MaxLength(150)]
    public string? ScientificName { get; set; }

    /// <summary>
    /// Plant family
    /// </summary>
    [Column("family")]
    [MaxLength(100)]
    public string? Family { get; set; }

    /// <summary>
    /// Description of the plant
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Image URL for the plant
    /// </summary>
    [Column("image_url")]
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Whether this plant type is active
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Created timestamp
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<PlantTypeDisease> PlantTypeDiseases { get; set; } = new List<PlantTypeDisease>();
}
