using GreenSpace.Application.DTOs.Report;
using GreenSpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    /// <summary>
    /// Admin reporting and analytics endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // =================================================================
        // REVENUE REPORTS
        // =================================================================

        /// <summary>
        /// Get comprehensive revenue report (all metrics)
        /// </summary>
        /// <param name="fromDate">Start date (default: 1 month ago)</param>
        /// <param name="toDate">End date (default: now)</param>
        /// <param name="groupBy">Group by: Day, Week, Month, Year (default: Day)</param>
        /// <returns>Full revenue report</returns>
        /// <response code="200">Report data</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpGet("revenue")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRevenueReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] ReportGroupBy groupBy = ReportGroupBy.Day)
        {
            var filter = new ReportFilterDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                GroupBy = groupBy
            };

            var result = await _reportService.GetRevenueReportAsync(filter);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get revenue summary (totals only)
        /// </summary>
        /// <param name="fromDate">Start date (default: 1 month ago)</param>
        /// <param name="toDate">End date (default: now)</param>
        /// <returns>Revenue summary</returns>
        /// <response code="200">Summary data</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpGet("revenue/summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRevenueSummary(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var filter = new ReportFilterDto
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _reportService.GetRevenueSummaryAsync(filter);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get revenue by time period (for charts)
        /// </summary>
        /// <param name="fromDate">Start date (default: 1 month ago)</param>
        /// <param name="toDate">End date (default: now)</param>
        /// <param name="groupBy">Group by: Day, Week, Month, Year (default: Day)</param>
        /// <returns>Time series data for charts</returns>
        /// <response code="200">Time series data</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpGet("revenue/by-time")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRevenueByTime(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] ReportGroupBy groupBy = ReportGroupBy.Day)
        {
            var filter = new ReportFilterDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                GroupBy = groupBy
            };

            var result = await _reportService.GetRevenueByTimeAsync(filter);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get revenue breakdown by category
        /// </summary>
        /// <param name="fromDate">Start date (default: 1 month ago)</param>
        /// <param name="toDate">End date (default: now)</param>
        /// <returns>Revenue by category</returns>
        /// <response code="200">Category breakdown</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpGet("revenue/by-category")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRevenueByCategory(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var filter = new ReportFilterDto
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _reportService.GetRevenueByCategoryAsync(filter);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get revenue breakdown by payment method
        /// </summary>
        /// <param name="fromDate">Start date (default: 1 month ago)</param>
        /// <param name="toDate">End date (default: now)</param>
        /// <returns>Revenue by payment method</returns>
        /// <response code="200">Payment method breakdown</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpGet("revenue/by-payment-method")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRevenueByPaymentMethod(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var filter = new ReportFilterDto
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _reportService.GetRevenueByPaymentMethodAsync(filter);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // =================================================================
        // PRODUCT REPORTS
        // =================================================================

        /// <summary>
        /// Get top selling products
        /// </summary>
        /// <param name="fromDate">Start date (default: 1 month ago)</param>
        /// <param name="toDate">End date (default: now)</param>
        /// <param name="top">Number of top products (default: 10)</param>
        /// <returns>Top selling products list</returns>
        /// <response code="200">Top products list</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpGet("products/top-selling")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTopSellingProducts(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int top = 10)
        {
            var filter = new ReportFilterDto
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _reportService.GetTopSellingProductsAsync(filter, top);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // =================================================================
        // PROMOTION REPORTS
        // =================================================================

        /// <summary>
        /// Get promotion/voucher usage statistics
        /// </summary>
        /// <param name="fromDate">Start date (default: 1 month ago)</param>
        /// <param name="toDate">End date (default: now)</param>
        /// <returns>Promotion usage statistics</returns>
        /// <response code="200">Promotion statistics</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpGet("promotions/usage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPromotionUsage(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var filter = new ReportFilterDto
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _reportService.GetPromotionUsageAsync(filter);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
