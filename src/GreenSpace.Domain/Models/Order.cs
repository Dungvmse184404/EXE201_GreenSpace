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

    // ============================================
    // Price Breakdown (Simplified)
    // ============================================

    /// <summary>
    /// Tổng giá sản phẩm (trước giảm giá)
    /// SubTotal = Sum(OrderItem.Price * OrderItem.Quantity)
    /// </summary>
    [Column("sub_total")]
    [Precision(10, 2)]
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Giảm giá sản phẩm (từ voucher hoặc promotion)
    /// </summary>
    [Column("discount")]
    [Precision(10, 2)]
    public decimal Discount { get; set; }

    /// <summary>
    /// Mã voucher đã áp dụng (optional)
    /// </summary>
    [Column("voucher_code")]
    [StringLength(50)]
    public string? VoucherCode { get; set; }

    /// <summary>
    /// Phí vận chuyển (không có giảm giá)
    /// </summary>
    [Column("shipping_fee")]
    [Precision(10, 2)]
    public decimal ShippingFee { get; set; }

    /// <summary>
    /// Tổng tiền (deprecated - giữ lại để backward compatible)
    /// Sẽ = FinalAmount sau khi migrate
    /// </summary>
    [Column("total_amount")]
    [Precision(10, 2)]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Số tiền cuối cùng khách phải trả
    /// FinalAmount = SubTotal - Discount + ShippingFee
    /// Payment gateway sẽ lấy số này
    /// </summary>
    [Column("final_amount")]
    [Precision(10, 2)]
    public decimal FinalAmount { get; set; }

    // ============================================
    // Order Info
    // ============================================

    [Column("status")]
    [StringLength(50)]
    public string? Status { get; set; }

    // ============================================
    // Shipping Info (Snapshot từ UserAddress + Recipient)
    // ============================================

    /// <summary>
    /// FK đến UserAddress đã chọn (optional, để tracking)
    /// </summary>
    [Column("shipping_address_id")]
    public Guid? ShippingAddressId { get; set; }

    /// <summary>
    /// Địa chỉ giao hàng (snapshot - không thay đổi khi UserAddress thay đổi)
    /// </summary>
    [Column("shipping_address")]
    [StringLength(500)]
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// Tên người nhận hàng
    /// </summary>
    [Column("recipient_name")]
    [StringLength(100)]
    public string? RecipientName { get; set; }

    /// <summary>
    /// Số điện thoại người nhận hàng
    /// </summary>
    [Column("recipient_phone")]
    [StringLength(20)]
    public string? RecipientPhone { get; set; }

    /// <summary>
    /// Ghi chú của khách hàng
    /// </summary>
    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("payment_expiry_at", TypeName = "timestamp without time zone")]
    public DateTime? PaymentExpiryAt { get; set; }

    [Column("stock_reserved")]
    public bool StockReserved { get; set; }

    // ============================================
    // Navigation Properties
    // ============================================

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [InverseProperty("Order")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("UserId")]
    [InverseProperty("Orders")]
    public virtual User User { get; set; } = null!;

    // ============================================
    // Computed Property (không lưu DB)
    // ============================================

    /// <summary>
    /// Tính lại FinalAmount từ các thành phần
    /// FinalAmount = SubTotal - Discount + ShippingFee
    /// </summary>
    [NotMapped]
    public decimal CalculatedFinalAmount =>
        SubTotal - Discount + ShippingFee;
}
