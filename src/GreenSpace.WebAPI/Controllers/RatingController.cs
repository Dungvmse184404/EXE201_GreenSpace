using GreenSpace.Application.DTOs.Rating;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GreenSpace.WebAPI.Controllers
{
    /// <summary>
    /// Product rating and review endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        /// <summary>
        /// Get all ratings for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>List of ratings</returns>
        /// <response code="200">List of ratings</response>
        /// <response code="400">Error occurred</response>
        [HttpGet("product/{productId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            var result = await _ratingService.GetByProductIdAsync(productId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get average rating for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Average rating and total count</returns>
        /// <response code="200">Average rating data</response>
        /// <response code="400">Error occurred</response>
        [HttpGet("product/{productId:guid}/average")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAverageRating(Guid productId)
        {
            var result = await _ratingService.GetAverageRatingAsync(productId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get current user's ratings
        /// </summary>
        /// <returns>List of user's ratings</returns>
        /// <response code="200">List of ratings</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("my-ratings")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyRatings()
        {
            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _ratingService.GetByUserIdAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get rating by ID
        /// </summary>
        /// <param name="id">Rating ID</param>
        /// <returns>Rating data</returns>
        /// <response code="200">Rating found</response>
        /// <response code="404">Rating not found</response>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _ratingService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Create a new rating
        /// </summary>
        /// <param name="dto">Rating data</param>
        /// <returns>Created rating</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/ratings
        ///     {
        ///         "productId": "guid",       // ID san pham can danh gia
        ///         "stars": 5,                // So sao (1-5)
        ///         "comment": "San pham tot!" // Nhan xet (optional, max 1000 chars)
        ///     }
        /// </remarks>
        /// <response code="201">Rating created successfully</response>
        /// <response code="400">Invalid data or already rated this product</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateRatingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            var result = await _ratingService.CreateAsync(dto, userId);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Data?.RatingId }, result)
                : BadRequest(result);
        }

        /// <summary>
        /// Update a rating (owner only)
        /// </summary>
        /// <param name="id">Rating ID to update</param>
        /// <param name="dto">Updated rating data</param>
        /// <returns>Updated rating</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/ratings/{id}
        ///     {
        ///         "stars": 4,                     // So sao moi (1-5)
        ///         "comment": "Updated comment"    // Nhan xet moi (optional)
        ///     }
        /// </remarks>
        /// <response code="200">Rating updated successfully</response>
        /// <response code="400">Invalid data or not owner</response>
        /// <response code="401">Unauthorized</response>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRatingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            var result = await _ratingService.UpdateAsync(id, dto, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete a rating (owner only)
        /// </summary>
        /// <param name="id">Rating ID to delete</param>
        /// <returns>Success message</returns>
        /// <response code="200">Rating deleted successfully</response>
        /// <response code="400">Not owner or rating not found</response>
        /// <response code="401">Unauthorized</response>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.GetUserId();
            var result = await _ratingService.DeleteAsync(id, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
