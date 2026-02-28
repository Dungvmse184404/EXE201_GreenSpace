using GreenSpace.Application.DTOs.Product;
using GreenSpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    /// <summary>
    /// Product management endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns>List of all products</returns>
        /// <response code="200">List of products</response>
        /// <response code="400">Error occurred</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _productService.GetAllAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">Product ID (GUID)</param>
        /// <returns>Product data with variants</returns>
        /// <response code="200">Product found</response>
        /// <response code="404">Product not found</response>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _productService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Create a new product (Admin/Staff only)
        /// </summary>
        /// <param name="dto">Product data</param>
        /// <returns>Created product</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/products
        ///     {
        ///         "name": "Cay xanh mini",           // Ten san pham
        ///         "description": "Mo ta san pham",   // Mo ta chi tiet
        ///         "basePrice": 150000,               // Gia goc (VND)
        ///         "thumbnailUrl": "https://...",     // URL anh dai dien
        ///         "categoryId": "guid",              // ID danh muc
        ///         "brandId": "guid"                  // ID thuong hieu (optional)
        ///     }
        /// </remarks>
        /// <response code="201">Product created successfully</response>
        /// <response code="400">Invalid data</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin/Staff only</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _productService.CreateAsync(dto);
            return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data?.ProductId }, result) : BadRequest(result);
        }

        /// <summary>
        /// Update a product (Admin/Staff only)
        /// </summary>
        /// <param name="id">Product ID to update</param>
        /// <param name="dto">Updated product data</param>
        /// <returns>Updated product</returns>
        /// <response code="200">Product updated successfully</response>
        /// <response code="400">Invalid data or product not found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin/Staff only</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _productService.UpdateAsync(id, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete a product (Admin only)
        /// </summary>
        /// <param name="id">Product ID to delete</param>
        /// <returns>Success message</returns>
        /// <response code="200">Product deleted successfully</response>
        /// <response code="400">Error occurred or product has dependencies</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _productService.DeleteAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
