using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("orders")]
public partial class Order
{
    [Key]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("total_amount")]
    [Precision(10, 2)]
    public decimal TotalAmount { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string? Status { get; set; }

    [Column("shipping_address")]
    [StringLength(255)]
    public string? ShippingAddress { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [InverseProperty("Order")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("Order")]
    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    [ForeignKey("UserId")]
    [InverseProperty("Orders")]
    public virtual User User { get; set; } = null!;

    [Column("payment_expiry_at", TypeName = "timestamp without time zone")]
    public DateTime? PaymentExpiryAt { get; set; }

    [Column("stock_reserved")]
    public bool StockReserved { get; set; }
}
