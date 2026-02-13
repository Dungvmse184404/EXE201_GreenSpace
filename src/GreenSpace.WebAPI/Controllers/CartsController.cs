using GreenSpace.Application.DTOs.Cart;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    /// <summary>
    /// Shopping cart management endpoints
    /// </summary>
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

        /// <summary>
        /// Get current user's cart
        /// </summary>
        /// <returns>Cart with items</returns>
        /// <response code="200">Cart data</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = User.GetUserId();
            var result = await _cartService.GetUserCartAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        /// <param name="dto">Item to add</param>
        /// <returns>Updated cart</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/carts/items
        ///     {
        ///         "variantId": "guid",    // ID cua product variant
        ///         "quantity": 2           // So luong them vao (mac dinh: 1)
        ///     }
        /// </remarks>
        /// <response code="200">Item added successfully</response>
        /// <response code="400">Invalid variant or out of stock</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddItem([FromBody] ModifyCartItemDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.GetUserId();
            var result = await _cartService.AddItemAsync(userId, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        /// <param name="dto">Item to remove</param>
        /// <returns>Updated cart</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/carts/items
        ///     {
        ///         "variantId": "guid",    // ID cua product variant
        ///         "quantity": 1           // So luong xoa (mac dinh: xoa het)
        ///     }
        /// </remarks>
        /// <response code="200">Item removed successfully</response>
        /// <response code="400">Item not in cart</response>
        /// <response code="401">Unauthorized</response>
        [HttpDelete("items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveItem([FromBody] ModifyCartItemDto dto)
        {
            var userId = User.GetUserId();
            var result = await _cartService.RemoveItemAsync(userId, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Clear all items from cart
        /// </summary>
        /// <returns>Empty cart</returns>
        /// <response code="200">Cart cleared successfully</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.GetUserId();
            var result = await _cartService.ClearCartAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
