using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("promotions")]
public partial class Promotion
{
    [Key]
    [Column("promotion_id")]
    public Guid PromotionId { get; set; }

    [Column("order_id")]
    public Guid? OrderId { get; set; }

    [Column("start_date", TypeName = "timestamp without time zone")]
    public DateTime? StartDate { get; set; }

    [Column("end_date", TypeName = "timestamp without time zone")]
    public DateTime? EndDate { get; set; }

    [Column("discount_type")]
    [StringLength(50)]
    public string? DiscountType { get; set; }

    [Column("discount_amount")]
    [Precision(10, 2)]
    public decimal? DiscountAmount { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("create_at", TypeName = "timestamp without time zone")]
    public DateTime? CreateAt { get; set; }

    [Column("update_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdateAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("Promotions")]
    public virtual Order? Order { get; set; }
}
