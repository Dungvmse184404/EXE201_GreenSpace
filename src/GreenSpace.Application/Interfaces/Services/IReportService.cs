using GreenSpace.Application.DTOs.Report;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IReportService
    {
        /// <summary>
        /// Lấy báo cáo doanh thu tổng hợp
        /// </summary>
        /// <param name="filter">Bộ lọc thời gian</param>
        /// <returns>Báo cáo doanh thu đầy đủ</returns>
        Task<IServiceResult<RevenueReportDto>> GetRevenueReportAsync(ReportFilterDto filter);

        /// <summary>
        /// Lấy tổng quan doanh thu (summary)
        /// </summary>
        Task<IServiceResult<RevenueSummaryDto>> GetRevenueSummaryAsync(ReportFilterDto filter);

        /// <summary>
        /// Lấy doanh thu theo thời gian
        /// </summary>
        Task<IServiceResult<List<RevenueByTimeDto>>> GetRevenueByTimeAsync(ReportFilterDto filter);

        /// <summary>
        /// Lấy doanh thu theo danh mục
        /// </summary>
        Task<IServiceResult<List<RevenueByCategoryDto>>> GetRevenueByCategoryAsync(ReportFilterDto filter);

        /// <summary>
        /// Lấy doanh thu theo phương thức thanh toán
        /// </summary>
        Task<IServiceResult<List<RevenueByPaymentMethodDto>>> GetRevenueByPaymentMethodAsync(ReportFilterDto filter);

        /// <summary>
        /// Lấy top sản phẩm bán chạy
        /// </summary>
        /// <param name="filter">Bộ lọc thời gian</param>
        /// <param name="top">Số lượng top (mặc định 10)</param>
        Task<IServiceResult<List<TopSellingProductDto>>> GetTopSellingProductsAsync(ReportFilterDto filter, int top = 10);

        /// <summary>
        /// Lấy thống kê sử dụng voucher
        /// </summary>
        Task<IServiceResult<List<PromotionUsageDto>>> GetPromotionUsageAsync(ReportFilterDto filter);
    }
}
