using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("user_address")]
[Index("UserId", Name = "IX_user_address_user_id")]
public partial class UserAddress
{
    [Key]
    [Column("address_id")]
    public Guid AddressId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    // ============================================
    // Địa chỉ chi tiết
    // ============================================

    /// <summary>
    /// Tỉnh/Thành phố
    /// </summary>
    [Column("province")]
    [StringLength(100)]
    public string Province { get; set; } = null!;

    /// <summary>
    /// Quận/Huyện
    /// </summary>
    [Column("district")]
    [StringLength(100)]
    public string District { get; set; } = null!;

    /// <summary>
    /// Phường/Xã
    /// </summary>
    [Column("ward")]
    [StringLength(100)]
    public string Ward { get; set; } = null!;

    /// <summary>
    /// Địa chỉ cụ thể (số nhà, tên đường, tòa nhà...)
    /// </summary>
    [Column("street_address")]
    [StringLength(255)]
    public string StreetAddress { get; set; } = null!;

    // ============================================
    // Meta
    // ============================================

    /// <summary>
    /// Nhãn địa chỉ: "Nhà", "Công ty", "Nhà bố mẹ"...
    /// </summary>
    [Column("label")]
    [StringLength(50)]
    public string? Label { get; set; }

    /// <summary>
    /// Địa chỉ mặc định
    /// </summary>
    [Column("is_default")]
    public bool IsDefault { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    // ============================================
    // Navigation
    // ============================================

    [ForeignKey("UserId")]
    [InverseProperty("UserAddresses")]
    public virtual User User { get; set; } = null!;

    // ============================================
    // Computed Property (không lưu DB)
    // ============================================

    /// <summary>
    /// Địa chỉ đầy đủ (generated từ các thành phần)
    /// </summary>
    [NotMapped]
    public string FullAddress => $"{StreetAddress}, {Ward}, {District}, {Province}";
}
