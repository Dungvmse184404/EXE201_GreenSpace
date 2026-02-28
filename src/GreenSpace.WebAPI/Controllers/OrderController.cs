using GreenSpace.Application.DTOs.Order;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    /// <summary>
    /// Order management endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Get current user's order history
        /// </summary>
        /// <returns>List of orders</returns>
        /// <response code="200">List of orders</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("my-orders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.GetUserId();
            var result = await _orderService.GetUserOrdersAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        /// <param name="id">Order ID (GUID)</param>
        /// <returns>Order details with items</returns>
        /// <response code="200">Order found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Order not found</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _orderService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        /// <param name="dto">Order data</param>
        /// <returns>Created order</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/orders
        ///     {
        ///         "addressId": "guid",              // ID dia chi da luu (optiona - Default: lấy từ Is_default trong userAddress)
        ///         "shippingAddress": "123 ABC...",  // Dia chi giao hang (bắt buộc nếu không có addressId ở trên)
        ///         "recipientName": "Nguyen Van A",  // Ten nguoi nhan (optional - Default: người đang đăng nhập)
        ///         "recipientPhone": "0901234567",   // SDT nguoi nhan (optional - Default: người đang đăng nhập)
        ///         "paymentMethod": "COD",           // Phuong thuc: "COD", "VNPAY", "PAYOS" (optional - Default: tự fill sau khi thanh toán)
        ///         "voucherCode": "SALE20",          // Ma giam gia (optional - nhập sai hoặc không nhập mặc định sẽ không áp dụng giảm giá)
        ///         "note": "Giao gio hanh chinh",    // Ghi chu (optional)
        ///         "items": [  // các sản phẩm trong đơn hàng 
        ///             { "variantId": "guid", "quantity": 2 }
        ///         ]
        ///     }
        /// </remarks>
        /// <response code="201">Order created successfully</response>
        /// <response code="400">Invalid data, out of stock, or invalid voucher</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.GetUserId();
            var result = await _orderService.CreateOrderAsync(dto, userId);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Data?.OrderId }, result)
                : BadRequest(result);
        }

        /// <summary>
        /// Update order status (Admin/Staff only)
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="dto">New status</param>
        /// <returns>Updated order</returns>
        /// <remarks>
        /// Available statuses: "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled"
        ///
        ///     PATCH /api/orders/{id}/status
        ///     {
        ///         "status": "Shipped"
        ///     }
        /// </remarks>
        /// <response code="200">Status updated successfully</response>
        /// <response code="400">Invalid status or transition not allowed</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin/Staff only</response>
        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "ADMIN,STAFF")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
        {
            var result = await _orderService.UpdateStatusAsync(id, dto.Status);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get all orders (Admin/Staff only)
        /// </summary>
        /// <returns>all orders</returns>
        /// <response code="200">Status updated successfully</response>
        /// <response code="400">Invalid status or transition not allowed</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin/Staff only</response>
        [HttpGet]
        [Authorize(Roles = "ADMIN,STAFF")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _orderService.GetAllOrderAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

    /// <summary>
    /// DTO for updating order status
    /// </summary>
    /// <param name="Status">New status: Pending, Confirmed, Shipped, Delivered, Cancelled</param>
    public record UpdateOrderStatusDto(string Status);
}
