using GreenSpace.Application.DTOs.ProductVariant;
using GreenSpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    /// <summary>
    /// Product variant management endpoints
    /// </summary>
    [ApiController]
    [Route("api/products/{productId:guid}/variants")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public class ProductVariantsController : ControllerBase
    {
        private readonly IProductVariantService _variantService;

        public ProductVariantsController(IProductVariantService variantService)
        {
            _variantService = variantService;
        }

        /// <summary>
        /// Get all variants of a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>List of variants</returns>
        /// <response code="200">List of variants</response>
        /// <response code="400">Error occurred</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            var result = await _variantService.GetByProductIdAsync(productId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get variant by ID
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="variantId">Variant ID</param>
        /// <returns>Variant data</returns>
        /// <response code="200">Variant found</response>
        /// <response code="404">Variant not found</response>
        [HttpGet("{variantId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid productId, Guid variantId)
        {
            var result = await _variantService.GetByIdAsync(variantId);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Create a new variant (Admin/Staff only)
        /// </summary>
        /// <param name="productId">Product ID from URL</param>
        /// <param name="dto">Variant data</param>
        /// <returns>Created variant</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/products/{productId}/variants
        ///     {
        ///         "productId": "guid",           // Phai trung voi URL
        ///         "sku": "PLANT-001-GREEN",      // Ma SKU unique
        ///         "price": 180000,               // Gia ban (VND)
        ///         "stockQuantity": 50,           // So luong ton kho
        ///         "imageUrl": "https://...",     // URL anh variant
        ///         "color": "Xanh la",            // Mau sac
        ///         "sizeOrModel": "S"             // Size/Model
        ///     }
        /// </remarks>
        /// <response code="201">Variant created successfully</response>
        /// <response code="400">Invalid data or Product ID mismatch</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin/Staff only</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(Guid productId, [FromBody] CreateProductVariantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.ProductId != productId)
                return BadRequest("Product ID mismatch");

            var result = await _variantService.CreateAsync(dto);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { productId, variantId = result.Data?.VariantId }, result)
                : BadRequest(result);
        }

        /// <summary>
        /// Update a variant (Admin/Staff only)
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="variantId">Variant ID to update</param>
        /// <param name="dto">Updated variant data</param>
        /// <returns>Updated variant</returns>
        /// <response code="200">Variant updated successfully</response>
        /// <response code="400">Invalid data or variant not found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin/Staff only</response>
        [HttpPut("{variantId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid productId, Guid variantId, [FromBody] UpdateProductVariantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _variantService.UpdateAsync(variantId, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete a variant (Admin only)
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="variantId">Variant ID to delete</param>
        /// <returns>Success message</returns>
        /// <response code="200">Variant deleted successfully</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpDelete("{variantId:guid}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid productId, Guid variantId)
        {
            var result = await _variantService.DeleteAsync(variantId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Update stock quantity (Admin/Staff only)
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="variantId">Variant ID</param>
        /// <param name="dto">New stock quantity</param>
        /// <returns>Updated variant</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PATCH /api/products/{productId}/variants/{variantId}/stock
        ///     {
        ///         "quantity": 100    // So luong ton kho moi
        ///     }
        /// </remarks>
        /// <response code="200">Stock updated successfully</response>
        /// <response code="400">Invalid data or variant not found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin/Staff only</response>
        [HttpPatch("{variantId:guid}/stock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateStock(Guid productId, Guid variantId, [FromBody] UpdateStockDto dto)
        {
            var result = await _variantService.UpdateStockAsync(variantId, dto.Quantity);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

    /// <summary>
    /// DTO for updating stock quantity
    /// </summary>
    /// <param name="Quantity">New stock quantity (>= 0)</param>
    public record UpdateStockDto(int Quantity);
}
