using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("order_items")]
[Index("OrderId", Name = "IX_order_items_order_id")]
[Index("VariantId", Name = "IX_order_items_variant_id")]
public partial class OrderItem
{
    [Key]
    [Column("item_id")]
    public Guid ItemId { get; set; }

    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Column("variant_id")]
    public Guid? VariantId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("price_at_purchase")]
    [Precision(10, 2)]
    public decimal PriceAtPurchase { get; set; }

    [Column("custom_data")]
    [StringLength(255)]
    public string? CustomData { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderItems")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("VariantId")]
    [InverseProperty("OrderItems")]
    public virtual ProductVariant? Variant { get; set; }
}
