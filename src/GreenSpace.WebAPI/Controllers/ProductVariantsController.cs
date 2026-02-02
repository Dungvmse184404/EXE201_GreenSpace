using GreenSpace.Application.DTOs.ProductVariant;
using GreenSpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            var result = await _variantService.GetByProductIdAsync(productId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{variantId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid productId, Guid variantId)
        {
            var result = await _variantService.GetByIdAsync(variantId);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid productId, [FromBody] CreateProductVariantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ensure ProductId matches route
            if (dto.ProductId != productId)
                return BadRequest("Product ID mismatch");

            var result = await _variantService.CreateAsync(dto);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { productId, variantId = result.Data?.VariantId }, result)
                : BadRequest(result);
        }

        [HttpPut("{variantId:guid}")]
        public async Task<IActionResult> Update(Guid productId, Guid variantId, [FromBody] UpdateProductVariantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _variantService.UpdateAsync(variantId, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{variantId:guid}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid productId, Guid variantId)
        {
            var result = await _variantService.DeleteAsync(variantId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{variantId:guid}/stock")]
        public async Task<IActionResult> UpdateStock(Guid productId, Guid variantId, [FromBody] UpdateStockDto dto)
        {
            var result = await _variantService.UpdateStockAsync(variantId, dto.Quantity);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

    public record UpdateStockDto(int Quantity);
}