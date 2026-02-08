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

    /// <summary>
    /// Mã voucher (unique, dùng để áp dụng)
    /// VD: "SALE20", "NEWUSER50"
    /// </summary>
    [Column("code")]
    [StringLength(50)]
    public string? Code { get; set; }

    /// <summary>
    /// Tên/Mô tả promotion
    /// VD: "Giảm 20% tối đa 50k"
    /// </summary>
    [Column("name")]
    [StringLength(100)]
    public string? Name { get; set; }

    /// <summary>
    /// Mô tả chi tiết
    /// </summary>
    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("start_date", TypeName = "timestamp without time zone")]
    public DateTime? StartDate { get; set; }

    [Column("end_date", TypeName = "timestamp without time zone")]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Loại giảm giá: "Percentage" hoặc "Fixed"
    /// </summary>
    [Column("discount_type")]
    [StringLength(50)]
    public string? DiscountType { get; set; }

    /// <summary>
    /// Giá trị giảm:
    /// - Nếu Percentage: giá trị % (VD: 20 = 20%)
    /// - Nếu Fixed: số tiền cố định (VD: 50000)
    /// </summary>
    [Column("discount_value")]
    [Precision(10, 2)]
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// Giảm tối đa (chỉ áp dụng cho Percentage)
    /// VD: 50000 = tối đa giảm 50,000đ
    /// </summary>
    [Column("max_discount")]
    [Precision(10, 2)]
    public decimal? MaxDiscount { get; set; }

    /// <summary>
    /// Giá trị đơn hàng tối thiểu để áp dụng
    /// VD: 100000 = đơn tối thiểu 100,000đ
    /// </summary>
    [Column("min_order_value")]
    [Precision(10, 2)]
    public decimal? MinOrderValue { get; set; }

    /// <summary>
    /// Số lần sử dụng tối đa (null = không giới hạn)
    /// </summary>
    [Column("max_usage")]
    public int? MaxUsage { get; set; }

    /// <summary>
    /// Số lần đã sử dụng
    /// </summary>
    [Column("used_count")]
    public int UsedCount { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    // ============================================
    // Backward compatible (deprecated - sẽ xóa sau khi migrate)
    // ============================================

    [Column("discount_amount")]
    [Precision(10, 2)]
    public decimal? DiscountAmount { get; set; }

    [Column("create_at", TypeName = "timestamp without time zone")]
    public DateTime? CreateAt { get; set; }

    [Column("update_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdateAt { get; set; }

    // ============================================
    // Helper Methods
    // ============================================

    /// <summary>
    /// Tính số tiền được giảm dựa trên SubTotal
    /// </summary>
    /// <param name="subTotal">Tổng giá sản phẩm</param>
    /// <returns>Số tiền được giảm</returns>
    public decimal CalculateDiscount(decimal subTotal)
    {
        if (!IsActive) return 0;

        // Kiểm tra min order value
        if (MinOrderValue.HasValue && subTotal < MinOrderValue.Value)
            return 0;

        // Kiểm tra max usage
        if (MaxUsage.HasValue && UsedCount >= MaxUsage.Value)
            return 0;

        // Kiểm tra thời gian
        var now = DateTime.UtcNow;
        if (StartDate.HasValue && now < StartDate.Value)
            return 0;
        if (EndDate.HasValue && now > EndDate.Value)
            return 0;

        decimal discount;

        if (DiscountType?.ToLower() == "percentage")
        {
            // Tính % của subtotal
            discount = subTotal * (DiscountValue / 100);

            // Áp dụng max discount nếu có
            if (MaxDiscount.HasValue && discount > MaxDiscount.Value)
                discount = MaxDiscount.Value;
        }
        else // Fixed
        {
            discount = DiscountValue;
        }

        // Không giảm quá subtotal
        return Math.Min(discount, subTotal);
    }

    /// <summary>
    /// Kiểm tra voucher có hợp lệ không
    /// </summary>
    [NotMapped]
    public bool IsValid
    {
        get
        {
            if (!IsActive) return false;
            if (MaxUsage.HasValue && UsedCount >= MaxUsage.Value) return false;

            var now = DateTime.UtcNow;
            if (StartDate.HasValue && now < StartDate.Value) return false;
            if (EndDate.HasValue && now > EndDate.Value) return false;

            return true;
        }
    }
}
