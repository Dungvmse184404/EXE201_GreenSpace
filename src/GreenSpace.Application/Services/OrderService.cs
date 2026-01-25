using AutoMapper;
using GreenSpace.Application.DTOs.Order;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IServiceResult<List<OrderDto>>> GetUserOrdersAsync(Guid userId)
        {
            try
            {
                var orders = await _unitOfWork.OrderRepository.GetAllQueryable()
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                var dtos = _mapper.Map<List<OrderDto>>(orders);
                return ServiceResult<List<OrderDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for user {UserId}", userId);
                return ServiceResult<List<OrderDto>>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<OrderDto>> GetByIdAsync(Guid orderId)
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetAllQueryable()
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                    return ServiceResult<OrderDto>.Failure("Order not found");

                var dto = _mapper.Map<OrderDto>(order);
                return ServiceResult<OrderDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                return ServiceResult<OrderDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<OrderDto>> CreateAsync(CreateOrderDto dto, Guid userId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var order = _mapper.Map<Order>(dto);
                order.UserId = userId;
                order.Status = "Pending";
                order.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.OrderRepository.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                var result = _mapper.Map<OrderDto>(order);
                return ServiceResult<OrderDto>.Success(result, "Order created");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error creating order");
                return ServiceResult<OrderDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<OrderDto>> UpdateStatusAsync(Guid orderId, string status)
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
                if (order == null)
                    return ServiceResult<OrderDto>.Failure("Order not found");

                order.Status = status;
                await _unitOfWork.OrderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<OrderDto>(order);
                return ServiceResult<OrderDto>.Success(result, "Order status updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status {OrderId}", orderId);
                return ServiceResult<OrderDto>.Failure($"Error: {ex.Message}");
            }
        }
    }
}

