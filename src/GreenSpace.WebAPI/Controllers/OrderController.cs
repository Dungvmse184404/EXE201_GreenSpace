using GreenSpace.Application.DTOs.Order;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GreenSpace.WebAPI.Controllers
{
    /// <summary>
    /// Quản lý đơn hàng (Orders) của hệ thống GreenSpace.
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
        /// Lấy danh sách lịch sử đơn hàng của người dùng hiện tại.
        /// </summary>
        /// <returns>Danh sách các đơn hàng đã thực hiện.</returns>
        /// <response code="200">Trả về danh sách đơn hàng thành công.</response>
        /// <response code="401">Chưa đăng nhập hoặc Token không hợp lệ.</response>
        [HttpGet("my-orders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.GetUserId();
            var result = await _orderService.GetUserOrdersAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một đơn hàng cụ thể theo ID.
        /// </summary>
        /// <param name="id" example="3fa85f64-5717-4562-b3fc-2c963f66afa6">ID của đơn hàng (GUID).</param>
        /// <response code="200">Tìm thấy đơn hàng.</response>
        /// <response code="404">Không tìm thấy đơn hàng với ID cung cấp.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _orderService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Tạo đơn hàng mới (Hỗ trợ cả mua trực tiếp hoặc mua từ giỏ hàng).
        /// </summary>
        /// <remarks>
        /// **Lưu ý:** truyền vào danh sách CreateOrderItemDto để mua nhiều item cho 1 order
        /// </remarks>
        /// <param name="dto">Thông tin đơn hàng cần tạo.</param>
        /// <response code="201">Tạo đơn hàng thành công.</response>
        /// <response code="400">Dữ liệu không hợp lệ hoặc lỗi logic (hết hàng, v.v.).</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        /// Cập nhật trạng thái đơn hàng (Chỉ dành cho Admin/Staff).
        /// </summary>
        /// <param name="id" example="3fa85f64-5717-4562-b3fc-2c963f66afa6">ID của đơn hàng.</param>
        /// <param name="dto">Trạng thái mới (e.g., Shipped, Cancelled, Completed).</param>
        /// <response code="200">Cập nhật thành công.</response>
        /// <response code="403">Không có quyền truy cập (không phải Admin/Staff).</response>
        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "ADMIN,STAFF")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
        {
            var result = await _orderService.UpdateStatusAsync(id, dto.Status);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

    /// <summary>
    /// DTO cập nhật trạng thái đơn hàng.
    /// </summary>
    /// <param name="Status" example="Shipped">Trạng thái đơn hàng muốn chuyển đổi.</param>
    public record UpdateOrderStatusDto(string Status);
}