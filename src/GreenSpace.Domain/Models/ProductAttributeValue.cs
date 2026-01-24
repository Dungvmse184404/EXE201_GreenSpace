using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("product_attribute_values")]
public partial class ProductAttributeValue
{
    [Key]
    [Column("value_id")]
    public Guid ValueId { get; set; }

    [Column("attribute_id")]
    public Guid AttributeId { get; set; }

    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Column("value")]
    [StringLength(255)]
    public string Value { get; set; } = null!;

    [ForeignKey("AttributeId")]
    [InverseProperty("ProductAttributeValues")]
    public virtual Attribute Attribute { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("ProductAttributeValues")]
    public virtual Product Product { get; set; } = null!;
}
