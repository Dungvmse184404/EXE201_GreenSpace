using AutoMapper;
using GreenSpace.Application.Common.Constants;
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
                    .AsNoTracking()
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Variant)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return ServiceResult<List<OrderDto>>.Success(_mapper.Map<List<OrderDto>>(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for user {UserId}", userId);
                return ServiceResult<List<OrderDto>>.Failure(ApiStatusCodes.InternalServerError, "Failed to retrieve your orders.");
            }
        }

        public async Task<IServiceResult<OrderDto>> GetByIdAsync(Guid orderId)
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetAllQueryable()
                    .AsNoTracking()
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Variant)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                    return ServiceResult<OrderDto>.Failure(ApiStatusCodes.NotFound, "Order not found.");

                return ServiceResult<OrderDto>.Success(_mapper.Map<OrderDto>(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                return ServiceResult<OrderDto>.Failure(ApiStatusCodes.InternalServerError, "Error retrieving order details.");
            }
        }

        public async Task<IServiceResult<OrderDto>> CreateAsync(CreateOrderDto dto, Guid userId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. Validate Stock for all items first
                foreach (var item in dto.Items)
                {
                    var variant = await _unitOfWork.ProductVariantRepository.GetByIdAsync(item.VariantId);
                    if (variant == null)
                        throw new Exception($"Variant {item.VariantId} not found.");

                    if (variant.StockQuantity < item.Quantity)
                        return ServiceResult<OrderDto>.Failure(ApiStatusCodes.BadRequest, $"Not enough stock for variant: {variant.Sku}");

                    // 2. Deduct Stock
                    variant.StockQuantity -= item.Quantity;
                    await _unitOfWork.ProductVariantRepository.UpdateAsync(variant);
                }

                // 3. Create Order
                var order = _mapper.Map<Order>(dto);
                order.UserId = userId;
                order.Status = "Pending"; // Use a Constant like OrderStatuses.Pending
                order.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.OrderRepository.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // 4. (Optional) Clear User Cart here if your logic requires it

                await _unitOfWork.CommitAsync();

                return ServiceResult<OrderDto>.Success(_mapper.Map<OrderDto>(order), "Order placed successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                return ServiceResult<OrderDto>.Failure(ApiStatusCodes.InternalServerError, "Order creation failed. Please try again.");
            }
        }

        public async Task<IServiceResult<OrderDto>> UpdateStatusAsync(Guid orderId, string status)
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
                if (order == null)
                    return ServiceResult<OrderDto>.Failure(ApiStatusCodes.NotFound, "Order not found.");

  
                order.Status = status;
                await _unitOfWork.OrderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<OrderDto>.Success(_mapper.Map<OrderDto>(order), $"Order status updated to {status}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for {OrderId}", orderId);
                return ServiceResult<OrderDto>.Failure(ApiStatusCodes.InternalServerError, "Failed to update order status.");
            }
        }
    }
}