using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("attributes")]
public partial class Attribute
{
    [Key]
    [Column("attribute_id")]
    public int AttributeId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("data_type")]
    [StringLength(50)]
    public string? DataType { get; set; }

    [InverseProperty("Attribute")]
    public virtual ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();
}
