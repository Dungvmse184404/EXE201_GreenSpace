using GreenSpace.Application.DTOs.Rating;
using GreenSpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GreenSpace.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [HttpGet("product/{productId:guid}")]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            var result = await _ratingService.GetByProductIdAsync(productId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("product/{productId:guid}/average")]
        public async Task<IActionResult> GetAverageRating(Guid productId)
        {
            var result = await _ratingService.GetAverageRatingAsync(productId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my-ratings")]
        [Authorize]
        public async Task<IActionResult> GetMyRatings()
        {
            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _ratingService.GetByUserIdAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _ratingService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateRatingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _ratingService.CreateAsync(dto, userId);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Data?.RatingId }, result)
                : BadRequest(result);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRatingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _ratingService.UpdateAsync(id, dto, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _ratingService.DeleteAsync(id, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}