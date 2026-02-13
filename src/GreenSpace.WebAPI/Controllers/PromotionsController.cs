using GreenSpace.Application.DTOs.Promotion;
using GreenSpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    /// <summary>
    /// Promotion and voucher management endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        /// <summary>
        /// Get all active promotions
        /// </summary>
        /// <returns>List of active promotions</returns>
        /// <response code="200">List of promotions</response>
        /// <response code="400">Error occurred</response>
        [HttpGet("active")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActive()
        {
            var result = await _promotionService.GetActivePromotionsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Create a new promotion (Admin only)
        /// </summary>
        /// <param name="dto">Promotion data</param>
        /// <returns>Created promotion</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/promotions
        ///     {
        ///         "code": "SALE20",              // Ma voucher (unique)
        ///         "name": "Giam 20%",            // Ten/Mo ta ngan
        ///         "description": "Giam 20%...",  // Mo ta chi tiet
        ///         "discountType": "Percentage",  // "Percentage" hoac "Fixed"
        ///         "discountValue": 20,           // Gia tri giam (20% hoac 20000d)
        ///         "maxDiscount": 50000,          // Giam toi da (cho Percentage)
        ///         "minOrderValue": 100000,       // Gia tri don toi thieu
        ///         "maxUsage": 100,               // So lan su dung toi da
        ///         "startDate": "2024-01-01",     // Ngay bat dau
        ///         "endDate": "2024-12-31"        // Ngay ket thuc
        ///     }
        /// </remarks>
        /// <response code="200">Promotion created successfully</response>
        /// <response code="400">Invalid data or code already exists</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreatePromotionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _promotionService.CreateAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
