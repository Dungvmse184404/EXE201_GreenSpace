using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.Report;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IUnitOfWork unitOfWork, ILogger<ReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Lấy báo cáo doanh thu tổng hợp
        /// </summary>
        public async Task<IServiceResult<RevenueReportDto>> GetRevenueReportAsync(ReportFilterDto filter)
        {
            try
            {
                var report = new RevenueReportDto
                {
                    Summary = (await GetRevenueSummaryAsync(filter)).Data ?? new RevenueSummaryDto(),
                    RevenueByTime = (await GetRevenueByTimeAsync(filter)).Data ?? new List<RevenueByTimeDto>(),
                    RevenueByCategory = (await GetRevenueByCategoryAsync(filter)).Data ?? new List<RevenueByCategoryDto>(),
                    RevenueByPaymentMethod = (await GetRevenueByPaymentMethodAsync(filter)).Data ?? new List<RevenueByPaymentMethodDto>(),
                    TopSellingProducts = (await GetTopSellingProductsAsync(filter, 10)).Data ?? new List<TopSellingProductDto>()
                };

                return ServiceResult<RevenueReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue report");
                return ServiceResult<RevenueReportDto>.Failure(ApiStatusCodes.InternalServerError, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy tổng quan doanh thu
        /// </summary>
        public async Task<IServiceResult<RevenueSummaryDto>> GetRevenueSummaryAsync(ReportFilterDto filter)
        {
            try
            {
                var (fromDate, toDate) = NormalizeDateRange(filter);

                var ordersQuery = _unitOfWork.OrderRepository.GetAllQueryable()
                    .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate);

                var orders = await ordersQuery.ToListAsync();

                var completedOrders = orders.Where(o => o.Status == "Completed").ToList();

                var summary = new RevenueSummaryDto
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalOrders = orders.Count,
                    CompletedOrders = completedOrders.Count,
                    ProcessingOrders = orders.Count(o => o.Status == "Processing"),
                    PendingOrders = orders.Count(o => o.Status == "Pending"),
                    CancelledOrders = orders.Count(o => o.Status == "Cancelled"),
                    TotalRevenue = completedOrders.Sum(o => o.FinalAmount),
                    TotalDiscount = completedOrders.Sum(o => o.Discount),
                    TotalShippingFee = completedOrders.Sum(o => o.ShippingFee),
                    AverageOrderValue = completedOrders.Count > 0
                        ? completedOrders.Average(o => o.FinalAmount)
                        : 0
                };

                return ServiceResult<RevenueSummaryDto>.Success(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue summary");
                return ServiceResult<RevenueSummaryDto>.Failure(ApiStatusCodes.InternalServerError, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy doanh thu theo thời gian
        /// </summary>
        public async Task<IServiceResult<List<RevenueByTimeDto>>> GetRevenueByTimeAsync(ReportFilterDto filter)
        {
            try
            {
                var (fromDate, toDate) = NormalizeDateRange(filter);

                var completedOrders = await _unitOfWork.OrderRepository.GetAllQueryable()
                    .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
                    .Where(o => o.Status == "Completed")
                    .ToListAsync();

                var result = filter.GroupBy switch
                {
                    ReportGroupBy.Week => GroupByWeek(completedOrders, fromDate, toDate),
                    ReportGroupBy.Month => GroupByMonth(completedOrders, fromDate, toDate),
                    ReportGroupBy.Year => GroupByYear(completedOrders, fromDate, toDate),
                    _ => GroupByDay(completedOrders, fromDate, toDate)
                };

                return ServiceResult<List<RevenueByTimeDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue by time");
                return ServiceResult<List<RevenueByTimeDto>>.Failure(ApiStatusCodes.InternalServerError, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy doanh thu theo danh mục
        /// </summary>
        public async Task<IServiceResult<List<RevenueByCategoryDto>>> GetRevenueByCategoryAsync(ReportFilterDto filter)
        {
            try
            {
                var (fromDate, toDate) = NormalizeDateRange(filter);

                // Lấy tất cả order items từ completed orders trong khoảng thời gian
                var orderItems = await _unitOfWork.OrderItemRepository.GetAllQueryable()
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Variant)
                        .ThenInclude(v => v!.Product)
                            .ThenInclude(p => p!.Category)
                    .Where(oi => oi.Order.CreatedAt >= fromDate && oi.Order.CreatedAt <= toDate)
                    .Where(oi => oi.Order.Status == "Completed")
                    .Where(oi => oi.Variant != null && oi.Variant.Product != null)
                    .ToListAsync();

                var totalRevenue = orderItems.Sum(oi => oi.PriceAtPurchase * oi.Quantity);

                var result = orderItems
                    .Where(oi => oi.Variant?.Product?.Category != null)
                    .GroupBy(oi => new
                    {
                        CategoryId = oi.Variant!.Product!.Category!.CategoryId,
                        CategoryName = oi.Variant.Product.Category.Name
                    })
                    .Select(g => new RevenueByCategoryDto
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.CategoryName,
                        Revenue = g.Sum(oi => oi.PriceAtPurchase * oi.Quantity),
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        Percentage = totalRevenue > 0
                            ? Math.Round(g.Sum(oi => oi.PriceAtPurchase * oi.Quantity) / totalRevenue * 100, 2)
                            : 0
                    })
                    .OrderByDescending(c => c.Revenue)
                    .ToList();

                return ServiceResult<List<RevenueByCategoryDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue by category");
                return ServiceResult<List<RevenueByCategoryDto>>.Failure(ApiStatusCodes.InternalServerError, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy doanh thu theo phương thức thanh toán
        /// </summary>
        public async Task<IServiceResult<List<RevenueByPaymentMethodDto>>> GetRevenueByPaymentMethodAsync(ReportFilterDto filter)
        {
            try
            {
                var (fromDate, toDate) = NormalizeDateRange(filter);

                var payments = await _unitOfWork.PaymentRepository.GetAllQueryable()
                    .Include(p => p.Order)
                    .Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate)
                    .Where(p => p.Status == "Success")
                    .ToListAsync();

                var totalRevenue = payments.Sum(p => p.Amount);

                var result = payments
                    .GroupBy(p => p.Gateway ?? "Unknown")
                    .Select(g => new RevenueByPaymentMethodDto
                    {
                        PaymentMethod = g.Key,
                        Revenue = g.Sum(p => p.Amount),
                        TransactionCount = g.Count(),
                        Percentage = totalRevenue > 0
                            ? Math.Round(g.Sum(p => p.Amount) / totalRevenue * 100, 2)
                            : 0
                    })
                    .OrderByDescending(p => p.Revenue)
                    .ToList();

                return ServiceResult<List<RevenueByPaymentMethodDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue by payment method");
                return ServiceResult<List<RevenueByPaymentMethodDto>>.Failure(ApiStatusCodes.InternalServerError, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy top sản phẩm bán chạy
        /// </summary>
        public async Task<IServiceResult<List<TopSellingProductDto>>> GetTopSellingProductsAsync(ReportFilterDto filter, int top = 10)
        {
            try
            {
                var (fromDate, toDate) = NormalizeDateRange(filter);

                var orderItems = await _unitOfWork.OrderItemRepository.GetAllQueryable()
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Variant)
                        .ThenInclude(v => v!.Product)
                    .Where(oi => oi.Order.CreatedAt >= fromDate && oi.Order.CreatedAt <= toDate)
                    .Where(oi => oi.Order.Status == "Completed")
                    .Where(oi => oi.Variant != null && oi.Variant.Product != null)
                    .ToListAsync();

                var result = orderItems
                    .GroupBy(oi => new
                    {
                        ProductId = oi.Variant!.Product!.ProductId,
                        ProductName = oi.Variant.Product.Name,
                        ThumbnailUrl = oi.Variant.Product.ThumbnailUrl
                    })
                    .Select(g => new TopSellingProductDto
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        ThumbnailUrl = g.Key.ThumbnailUrl,
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.PriceAtPurchase * oi.Quantity)
                    })
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(top)
                    .ToList();

                return ServiceResult<List<TopSellingProductDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top selling products");
                return ServiceResult<List<TopSellingProductDto>>.Failure(ApiStatusCodes.InternalServerError, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thống kê sử dụng voucher
        /// </summary>
        public async Task<IServiceResult<List<PromotionUsageDto>>> GetPromotionUsageAsync(ReportFilterDto filter)
        {
            try
            {
                var (fromDate, toDate) = NormalizeDateRange(filter);

                // Lấy tất cả orders có voucher code trong khoảng thời gian
                var ordersWithVoucher = await _unitOfWork.OrderRepository.GetAllQueryable()
                    .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
                    .Where(o => o.Status == "Completed")
                    .Where(o => !string.IsNullOrEmpty(o.VoucherCode))
                    .ToListAsync();

                // Lấy thông tin promotions
                var promotions = await _unitOfWork.PromotionRepository.GetAllQueryable()
                    .ToListAsync();

                var promotionDict = promotions.ToDictionary(p => p.Code?.ToUpper() ?? "", p => p);

                var result = ordersWithVoucher
                    .GroupBy(o => o.VoucherCode!.ToUpper())
                    .Select(g =>
                    {
                        promotionDict.TryGetValue(g.Key, out var promo);
                        return new PromotionUsageDto
                        {
                            PromotionId = promo?.PromotionId ?? Guid.Empty,
                            Code = g.Key,
                            Name = promo?.Name,
                            UsageCount = g.Count(),
                            TotalDiscountAmount = g.Sum(o => o.Discount)
                        };
                    })
                    .OrderByDescending(p => p.UsageCount)
                    .ToList();

                return ServiceResult<List<PromotionUsageDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting promotion usage");
                return ServiceResult<List<PromotionUsageDto>>.Failure(ApiStatusCodes.InternalServerError, $"Lỗi: {ex.Message}");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Normalize date range - mặc định 1 tháng gần nhất
        /// </summary>
        private (DateTime fromDate, DateTime toDate) NormalizeDateRange(ReportFilterDto filter)
        {
            // ToDate: mặc định là thời điểm hiện tại
            var toDate = filter.ToDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.UtcNow;

            // FromDate: mặc định là 1 tháng trước
            var fromDate = filter.FromDate?.Date ?? toDate.AddMonths(-1);

            return (fromDate, toDate);
        }

        /// <summary>
        /// Group revenue by day
        /// </summary>
        private List<RevenueByTimeDto> GroupByDay(List<Domain.Models.Order> orders, DateTime fromDate, DateTime toDate)
        {
            var result = new List<RevenueByTimeDto>();

            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                var dayOrders = orders.Where(o => o.CreatedAt?.Date == date).ToList();
                result.Add(new RevenueByTimeDto
                {
                    Label = date.ToString("dd/MM/yyyy"),
                    Date = date,
                    Revenue = dayOrders.Sum(o => o.FinalAmount),
                    OrderCount = dayOrders.Count
                });
            }

            return result;
        }

        /// <summary>
        /// Group revenue by week
        /// </summary>
        private List<RevenueByTimeDto> GroupByWeek(List<Domain.Models.Order> orders, DateTime fromDate, DateTime toDate)
        {
            var result = new List<RevenueByTimeDto>();
            var currentWeekStart = fromDate.Date.AddDays(-(int)fromDate.DayOfWeek + 1); // Monday

            while (currentWeekStart <= toDate)
            {
                var weekEnd = currentWeekStart.AddDays(6);
                var weekOrders = orders.Where(o =>
                    o.CreatedAt?.Date >= currentWeekStart &&
                    o.CreatedAt?.Date <= weekEnd).ToList();

                result.Add(new RevenueByTimeDto
                {
                    Label = $"Tuần {GetWeekOfYear(currentWeekStart)} ({currentWeekStart:dd/MM} - {weekEnd:dd/MM})",
                    Date = currentWeekStart,
                    Revenue = weekOrders.Sum(o => o.FinalAmount),
                    OrderCount = weekOrders.Count
                });

                currentWeekStart = currentWeekStart.AddDays(7);
            }

            return result;
        }

        /// <summary>
        /// Group revenue by month
        /// </summary>
        private List<RevenueByTimeDto> GroupByMonth(List<Domain.Models.Order> orders, DateTime fromDate, DateTime toDate)
        {
            var result = new List<RevenueByTimeDto>();
            var currentMonth = new DateTime(fromDate.Year, fromDate.Month, 1);

            while (currentMonth <= toDate)
            {
                var monthEnd = currentMonth.AddMonths(1).AddDays(-1);
                var monthOrders = orders.Where(o =>
                    o.CreatedAt?.Year == currentMonth.Year &&
                    o.CreatedAt?.Month == currentMonth.Month).ToList();

                result.Add(new RevenueByTimeDto
                {
                    Label = currentMonth.ToString("MM/yyyy"),
                    Date = currentMonth,
                    Revenue = monthOrders.Sum(o => o.FinalAmount),
                    OrderCount = monthOrders.Count
                });

                currentMonth = currentMonth.AddMonths(1);
            }

            return result;
        }

        /// <summary>
        /// Group revenue by year
        /// </summary>
        private List<RevenueByTimeDto> GroupByYear(List<Domain.Models.Order> orders, DateTime fromDate, DateTime toDate)
        {
            var result = new List<RevenueByTimeDto>();

            for (var year = fromDate.Year; year <= toDate.Year; year++)
            {
                var yearOrders = orders.Where(o => o.CreatedAt?.Year == year).ToList();
                result.Add(new RevenueByTimeDto
                {
                    Label = year.ToString(),
                    Date = new DateTime(year, 1, 1),
                    Revenue = yearOrders.Sum(o => o.FinalAmount),
                    OrderCount = yearOrders.Count
                });
            }

            return result;
        }

        /// <summary>
        /// Get week number of year
        /// </summary>
        private int GetWeekOfYear(DateTime date)
        {
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        #endregion
    }
}
