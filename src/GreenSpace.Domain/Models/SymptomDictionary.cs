using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSpace.Domain.Models;

/// <summary>
/// Dictionary of plant symptoms for matching user descriptions
/// </summary>
[Table("symptom_dictionary")]
public class SymptomDictionary
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Canonical symptom name (e.g., "đốm nâu")
    /// </summary>
    [Column("canonical_name")]
    [StringLength(100)]
    [Required]
    public string CanonicalName { get; set; } = null!;

    /// <summary>
    /// Synonyms and variations (e.g., ["dom nau", "vết nâu", "chấm nâu"])
    /// </summary>
    [Column("synonyms")]
    public List<string> Synonyms { get; set; } = new();

    /// <summary>
    /// Category: leaf, stem, root, flower, fruit, general
    /// </summary>
    [Column("category")]
    [StringLength(50)]
    public string? Category { get; set; }

    /// <summary>
    /// English translation for reference
    /// </summary>
    [Column("english_name")]
    [StringLength(100)]
    public string? EnglishName { get; set; }

    /// <summary>
    /// When this entry was created
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
