using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("product_variants")]
[Index("Sku", Name = "product_variants_sku_key", IsUnique = true)]
public partial class ProductVariant
{
    [Key]
    [Column("variant_id")]
    public int VariantId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("sku")]
    [StringLength(100)]
    public string? Sku { get; set; }

    [Column("price")]
    [Precision(10, 2)]
    public decimal Price { get; set; }

    [Column("stock_quantity")]
    public int? StockQuantity { get; set; }

    [Column("image_url")]
    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [Column("color")]
    [StringLength(50)]
    public string? Color { get; set; }

    [Column("size_or_model")]
    [StringLength(50)]
    public string? SizeOrModel { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Variant")]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [InverseProperty("Variant")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [ForeignKey("ProductId")]
    [InverseProperty("ProductVariants")]
    public virtual Product Product { get; set; } = null!;
}
