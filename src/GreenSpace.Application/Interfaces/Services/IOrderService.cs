using GreenSpace.Application.DTOs.Order;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<IServiceResult<List<OrderDto>>> GetUserOrdersAsync(Guid userId);
        Task<IServiceResult<OrderDto>> GetByIdAsync(Guid orderId);
        Task<IServiceResult<OrderDto>> CreateOrderAsync(CreateOrderDto dto, Guid userId);
        Task<IServiceResult<OrderDto>> UpdateStatusAsync(Guid orderId, string status);
        Task<IServiceResult<List<OrderDto>>> GetAllOrderAsync();
    }
}
