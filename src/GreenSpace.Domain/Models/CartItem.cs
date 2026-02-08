using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("cart_items")]
[Index("CartId", Name = "IX_cart_items_cart_id")]
[Index("VariantId", Name = "IX_cart_items_variant_id")]
public partial class CartItem
{
    [Key]
    [Column("cart_item_id")]
    public Guid CartItemId { get; set; }

    [Column("cart_id")]
    public Guid CartId { get; set; }

    [Column("variant_id")]
    public Guid VariantId { get; set; }

    [Column("quantity")]
    public int? Quantity { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CartId")]
    [InverseProperty("CartItems")]
    public virtual Cart Cart { get; set; } = null!;

    [ForeignKey("VariantId")]
    [InverseProperty("CartItems")]
    public virtual ProductVariant Variant { get; set; } = null!;
}
