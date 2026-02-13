using System.ComponentModel.DataAnnotations;

namespace GreenSpace.Application.DTOs.UserAddress;

// ============================================
// Request DTOs
// ============================================

/// <summary>
/// DTO để tạo địa chỉ mới
/// </summary>
public class CreateUserAddressDto
{
    [Required(ErrorMessage = "Province is required")]
    [StringLength(100, ErrorMessage = "Province cannot exceed 100 characters")]
    public string Province { get; set; } = string.Empty;

    [Required(ErrorMessage = "District is required")]
    [StringLength(100, ErrorMessage = "District cannot exceed 100 characters")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ward is required")]
    [StringLength(100, ErrorMessage = "Ward cannot exceed 100 characters")]
    public string Ward { get; set; } = string.Empty;

    [Required(ErrorMessage = "Street address is required")]
    [StringLength(255, ErrorMessage = "Street address cannot exceed 255 characters")]
    public string StreetAddress { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Label cannot exceed 50 characters")]
    public string? Label { get; set; }

    /// <summary>
    /// Đặt làm địa chỉ mặc định
    /// </summary>
    public bool IsDefault { get; set; }
}

/// <summary>
/// DTO để cập nhật địa chỉ
/// </summary>
public class UpdateUserAddressDto
{
    [Required(ErrorMessage = "Province is required")]
    [StringLength(100, ErrorMessage = "Province cannot exceed 100 characters")]
    public string Province { get; set; } = string.Empty;

    [Required(ErrorMessage = "District is required")]
    [StringLength(100, ErrorMessage = "District cannot exceed 100 characters")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ward is required")]
    [StringLength(100, ErrorMessage = "Ward cannot exceed 100 characters")]
    public string Ward { get; set; } = string.Empty;

    [Required(ErrorMessage = "Street address is required")]
    [StringLength(255, ErrorMessage = "Street address cannot exceed 255 characters")]
    public string StreetAddress { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Label cannot exceed 50 characters")]
    public string? Label { get; set; }

    /// <summary>
    /// Đặt làm địa chỉ mặc định
    /// </summary>
    public bool IsDefault { get; set; }
}

// ============================================
// Response DTOs
// ============================================

/// <summary>
/// DTO trả về thông tin địa chỉ
/// </summary>
public class UserAddressResponseDto
{
    public Guid AddressId { get; set; }
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
