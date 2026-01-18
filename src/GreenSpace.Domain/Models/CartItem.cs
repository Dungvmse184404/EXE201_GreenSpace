using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("cart_items")]
public partial class CartItem
{
    [Key]
    [Column("cart_item_id")]
    public int CartItemId { get; set; }

    [Column("cart_id")]
    public int CartId { get; set; }

    [Column("variant_id")]
    public int VariantId { get; set; }

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
