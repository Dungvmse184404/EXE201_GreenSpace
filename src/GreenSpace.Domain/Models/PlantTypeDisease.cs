using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSpace.Domain.Models;

/// <summary>
/// Junction table linking plant types to diseases
/// </summary>
[Table("plant_type_diseases")]
public class PlantTypeDisease
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to PlantType
    /// </summary>
    [Column("plant_type_id")]
    public Guid PlantTypeId { get; set; }

    /// <summary>
    /// Foreign key to Disease
    /// </summary>
    [Column("disease_id")]
    public Guid DiseaseId { get; set; }

    /// <summary>
    /// How common this disease is for this plant type
    /// Common, Rare, Seasonal
    /// </summary>
    [Column("prevalence")]
    [MaxLength(20)]
    public string? Prevalence { get; set; }

    /// <summary>
    /// Additional notes about this disease for this plant
    /// </summary>
    [Column("notes")]
    public string? Notes { get; set; }

    // Navigation properties
    [ForeignKey("PlantTypeId")]
    public virtual PlantType PlantType { get; set; } = null!;

    [ForeignKey("DiseaseId")]
    public virtual Disease Disease { get; set; } = null!;
}
