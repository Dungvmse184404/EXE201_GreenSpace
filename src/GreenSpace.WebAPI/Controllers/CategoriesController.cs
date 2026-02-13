using GreenSpace.Application.DTOs.Category;
using GreenSpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    /// <summary>
    /// Category management endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of categories</returns>
        /// <response code="200">List of categories</response>
        /// <response code="400">Error occurred</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <param name="id">Category ID (GUID)</param>
        /// <returns>Category data</returns>
        /// <response code="200">Category found</response>
        /// <response code="404">Category not found</response>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Create a new category (Admin/Staff only)
        /// </summary>
        /// <param name="dto">Category data</param>
        /// <returns>Created category</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/categories
        ///     {
        ///         "name": "Cay canh",        // Ten danh muc
        ///         "slug": "cay-canh",        // URL slug (tu dong tao neu bo trong)
        ///         "parentId": "guid"         // ID danh muc cha (optional)
        ///     }
        /// </remarks>
        /// <response code="201">Category created successfully</response>
        /// <response code="400">Invalid data</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin/Staff only</response>
        [HttpPost]
        [Authorize(Roles = "ADMIN,STAFF")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _categoryService.CreateAsync(dto);
            return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data?.CategoryId }, result) : BadRequest(result);
        }

        /// <summary>
        /// Update a category (Admin/Staff only)
        /// </summary>
        /// <param name="id">Category ID to update</param>
        /// <param name="dto">Updated category data</param>
        /// <returns>Updated category</returns>
        /// <response code="200">Category updated successfully</response>
        /// <response code="400">Invalid data or category not found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin/Staff only</response>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _categoryService.UpdateAsync(id, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete a category (Admin only)
        /// </summary>
        /// <param name="id">Category ID to delete</param>
        /// <returns>Success message</returns>
        /// <response code="200">Category deleted successfully</response>
        /// <response code="400">Error occurred or category has products</response>
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
            var result = await _categoryService.DeleteAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
