using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.Promotion
{
    public class PromotionDto
    {
        public Guid PromotionId { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal? MinOrderValue { get; set; }
        public int? MaxUsage { get; set; }
        public int UsedCount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }

        // Backward compatible
        public decimal? DiscountAmount { get; set; }
    }

    public class CreatePromotionDto
    {
        /// <summary>
        /// Mã voucher (unique)
        /// VD: "SALE20", "NEWUSER"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên/Mô tả ngắn
        /// VD: "Giảm 20% tối đa 50k"
        /// </summary>
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Loại giảm giá: "Percentage" hoặc "Fixed"
        /// </summary>
        [Required]
        public string DiscountType { get; set; } = "Percentage";

        /// <summary>
        /// Giá trị giảm:
        /// - Percentage: số % (VD: 20 = 20%)
        /// - Fixed: số tiền cố định (VD: 50000)
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// Giảm tối đa (chỉ dùng cho Percentage)
        /// VD: 50000 = tối đa giảm 50,000đ
        /// </summary>
        public decimal? MaxDiscount { get; set; }

        /// <summary>
        /// Giá trị đơn hàng tối thiểu
        /// VD: 100000 = đơn tối thiểu 100k
        /// </summary>
        public decimal? MinOrderValue { get; set; }

        /// <summary>
        /// Số lần sử dụng tối đa (null = không giới hạn)
        /// </summary>
        public int? MaxUsage { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Backward compatible
        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; }
    }

    /// <summary>
    /// Kết quả tính discount
    /// </summary>
    public class DiscountResultDto
    {
        /// <summary>
        /// Mã voucher đã áp dụng
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên voucher
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Loại giảm giá
        /// </summary>
        public string? DiscountType { get; set; }

        /// <summary>
        /// Giá trị giảm (% hoặc số tiền)
        /// </summary>
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// Số tiền được giảm thực tế
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// SubTotal ban đầu
        /// </summary>
        public decimal OriginalSubTotal { get; set; }

        /// <summary>
        /// SubTotal sau khi giảm
        /// </summary>
        public decimal FinalSubTotal { get; set; }
    }

    /// <summary>
    /// Request để validate/tính discount
    /// </summary>
    public class ValidateVoucherDto
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal SubTotal { get; set; }
    }
}
