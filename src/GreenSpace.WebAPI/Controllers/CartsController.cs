using GreenSpace.Application.DTOs.Cart;
using GreenSpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GreenSpace.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _cartService.GetUserCartAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _cartService.AddItemAsync(userId, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("items/{cartItemId:guid}")]
        public async Task<IActionResult> RemoveItem(Guid cartItemId)
        {
            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _cartService.RemoveItemAsync(userId, cartItemId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _cartService.ClearCartAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}