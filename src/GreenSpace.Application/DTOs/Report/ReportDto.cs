using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GreenSpace.Application.DTOs.Report
{
    // =================================================================
    // ENUMS
    // =================================================================

    /// <summary>
    /// Enum để nhóm dữ liệu theo thời gian
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReportGroupBy
    {
        Day,
        Week,
        Month,
        Year
    }

    // =================================================================
    // REQUEST DTOs
    // =================================================================

    /// <summary>
    /// DTO để filter report theo khoảng thời gian
    /// </summary>
    public class ReportFilterDto
    {
        /// <summary>
        /// Ngày bắt đầu (mặc định: 1 tháng trước)
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Ngày kết thúc (mặc định: thời điểm hiện tại)
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Nhóm theo: Day, Week, Month, Year (mặc định: Day)
        /// </summary>
        public ReportGroupBy GroupBy { get; set; } = ReportGroupBy.Day;
    }

    // =================================================================
    // RESPONSE DTOs - REVENUE
    // =================================================================

    /// <summary>
    /// Tổng quan doanh thu (Dashboard summary)
    /// </summary>
    public class RevenueSummaryDto
    {
        /// <summary>
        /// Tổng doanh thu (chỉ tính đơn Completed)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Tổng số đơn hàng
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// Số đơn hoàn thành
        /// </summary>
        public int CompletedOrders { get; set; }

        /// <summary>
        /// Số đơn đang xử lý
        /// </summary>
        public int ProcessingOrders { get; set; }

        /// <summary>
        /// Số đơn hủy
        /// </summary>
        public int CancelledOrders { get; set; }

        /// <summary>
        /// Số đơn chờ thanh toán
        /// </summary>
        public int PendingOrders { get; set; }

        /// <summary>
        /// Giá trị đơn hàng trung bình (AOV)
        /// </summary>
        public decimal AverageOrderValue { get; set; }

        /// <summary>
        /// Tổng giảm giá đã áp dụng
        /// </summary>
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// Tổng phí ship thu được
        /// </summary>
        public decimal TotalShippingFee { get; set; }

        /// <summary>
        /// Khoảng thời gian báo cáo
        /// </summary>
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    /// <summary>
    /// Doanh thu theo thời gian (để vẽ chart)
    /// </summary>
    public class RevenueByTimeDto
    {
        /// <summary>
        /// Nhãn thời gian (VD: "2024-01-15", "Tuần 3", "Tháng 1")
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Ngày/thời điểm cụ thể
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Doanh thu trong khoảng thời gian này
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// Số đơn hàng
        /// </summary>
        public int OrderCount { get; set; }
    }

    /// <summary>
    /// Doanh thu theo danh mục sản phẩm
    /// </summary>
    public class RevenueByCategoryDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Doanh thu từ danh mục này
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// Số lượng sản phẩm đã bán
        /// </summary>
        public int QuantitySold { get; set; }

        /// <summary>
        /// Phần trăm đóng góp vào tổng doanh thu
        /// </summary>
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Doanh thu theo phương thức thanh toán
    /// </summary>
    public class RevenueByPaymentMethodDto
    {
        /// <summary>
        /// Phương thức thanh toán (VNPay, PayOS)
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Doanh thu
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// Số giao dịch
        /// </summary>
        public int TransactionCount { get; set; }

        /// <summary>
        /// Phần trăm
        /// </summary>
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Top sản phẩm bán chạy
    /// </summary>
    public class TopSellingProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }

        /// <summary>
        /// Tổng số lượng đã bán
        /// </summary>
        public int QuantitySold { get; set; }

        /// <summary>
        /// Doanh thu từ sản phẩm này
        /// </summary>
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Thống kê voucher/promotion đã sử dụng
    /// </summary>
    public class PromotionUsageDto
    {
        public Guid PromotionId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Name { get; set; }

        /// <summary>
        /// Số lần sử dụng
        /// </summary>
        public int UsageCount { get; set; }

        /// <summary>
        /// Tổng giá trị giảm giá
        /// </summary>
        public decimal TotalDiscountAmount { get; set; }
    }

    /// <summary>
    /// Response tổng hợp cho Revenue Report
    /// </summary>
    public class RevenueReportDto
    {
        /// <summary>
        /// Tổng quan
        /// </summary>
        public RevenueSummaryDto Summary { get; set; } = new();

        /// <summary>
        /// Doanh thu theo thời gian
        /// </summary>
        public List<RevenueByTimeDto> RevenueByTime { get; set; } = new();

        /// <summary>
        /// Doanh thu theo danh mục
        /// </summary>
        public List<RevenueByCategoryDto> RevenueByCategory { get; set; } = new();

        /// <summary>
        /// Doanh thu theo phương thức thanh toán
        /// </summary>
        public List<RevenueByPaymentMethodDto> RevenueByPaymentMethod { get; set; } = new();

        /// <summary>
        /// Top sản phẩm bán chạy
        /// </summary>
        public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
    }
}
