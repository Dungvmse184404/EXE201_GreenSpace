using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("products")]
public partial class Product
{
    [Key]
    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("brand_id")]
    public int? BrandId { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("base_price")]
    [Precision(10, 2)]
    public decimal BasePrice { get; set; }

    [Column("thumbnail_url")]
    [StringLength(255)]
    public string? ThumbnailUrl { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();

    [InverseProperty("Product")]
    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    [InverseProperty("Product")]
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
