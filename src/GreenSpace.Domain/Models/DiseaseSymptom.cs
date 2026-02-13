using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSpace.Domain.Models;

/// <summary>
/// Junction table linking diseases to symptoms with weights
/// </summary>
[Table("disease_symptoms")]
public class DiseaseSymptom
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to Disease
    /// </summary>
    [Column("disease_id")]
    public Guid DiseaseId { get; set; }

    /// <summary>
    /// Foreign key to SymptomDictionary
    /// </summary>
    [Column("symptom_id")]
    public Guid SymptomId { get; set; }

    /// <summary>
    /// Whether this is a primary/key symptom for the disease
    /// Primary symptoms get bonus weight in matching
    /// </summary>
    [Column("is_primary")]
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// Weight of this symptom for the disease (0.1 - 2.0)
    /// Higher weight = more important for diagnosis
    /// </summary>
    [Column("weight")]
    public decimal Weight { get; set; } = 1.0m;

    /// <summary>
    /// Which plant part this symptom affects (leaf, stem, root, fruit, flower, general)
    /// Used for more accurate matching when user specifies plant part
    /// Null means the symptom can appear on any plant part
    /// </summary>
    [Column("affected_part")]
    [StringLength(20)]
    public string? AffectedPart { get; set; }

    // Navigation properties
    [ForeignKey("DiseaseId")]
    public virtual Disease Disease { get; set; } = null!;

    [ForeignKey("SymptomId")]
    public virtual SymptomDictionary Symptom { get; set; } = null!;
}
