using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("payments")]
public partial class Payment
{
    [Key]
    [Column("payment_id")]
    public Guid PaymentId { get; set; }

    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Column("payment_method")]
    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [Column("transaction_ref")]
    [StringLength(100)]
    public string TransactionRef { get; set; } = string.Empty;

    [Column("gateway")]
    [StringLength(50)]
    public string Gateway { get; set; } = string.Empty;

    [Column("amount")]
    [Precision(10, 2)]
    public decimal Amount { get; set; }

    [Column("transaction_code")]
    [StringLength(100)]
    public string? TransactionCode { get; set; }

    [Column("bank_code")]
    [StringLength(50)]
    public string? BankCode { get; set; }

    [Column("payment_url")]
    [StringLength(1000)]
    public string? PaymentUrl { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string? Status { get; set; }

    [Column("response_code")]
    [StringLength(20)]
    public string? ResponseCode { get; set; }

    [Column("response_message")]
    [StringLength(255)]
    public string? ResponseMessage { get; set; }

    [Column("card_type")]
    [StringLength(50)]
    public string? CardType { get; set; }

    [Column("paid_at", TypeName = "timestamp without time zone")]
    public DateTime? PaidAt { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("expired_at", TypeName = "timestamp without time zone")]
    public DateTime? ExpiredAt { get; set; } // Payment link expiry

    [ForeignKey("OrderId")]
    [InverseProperty("Payments")]
    public virtual Order Order { get; set; } = null!;
}
