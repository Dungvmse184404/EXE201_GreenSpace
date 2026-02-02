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


        //hàm này cần sửa lại logic
        public async Task<IServiceResult<OrderDto>> CreateAsync(CreateOrderDto dto, Guid userId)
        {
            try
            {
                return await _unitOfWork.ExecuteStrategyAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        var variantIds = dto.Items.Select(i => i.VariantId).ToList();
                        var variants = await _unitOfWork.ProductVariantRepository.GetAllQueryable()
                            .Where(v => variantIds.Contains(v.VariantId))
                            .ToListAsync();

                        if (variants.Count != dto.Items.Count)
                        {
                            await _unitOfWork.RollbackAsync();
                            return ServiceResult<OrderDto>.Failure(
                                ApiStatusCodes.NotFound,
                                ApiMessages.ProductVariant.NotFound);
                        }
                        var variantDict = variants.ToDictionary(v => v.VariantId);
 
                        decimal totalAmount = 0;
                        var orderItems = new List<OrderItem>();

                        foreach (var item in dto.Items)
                        {
                            var variant = variantDict[item.VariantId];

                            // Check stock
                            if (variant.StockQuantity < item.Quantity)
                            {
                                await _unitOfWork.RollbackAsync();
                                return ServiceResult<OrderDto>.Failure(
                                    ApiStatusCodes.Conflict,
                                    $"{variant.Sku}: {ApiMessages.ProductVariant.InsufficientStock}");
                            }

                            // Deduct stock
                            variant.StockQuantity -= item.Quantity;
                            variant.UpdatedAt = DateTime.UtcNow;

                            // Create order item
                            var orderItem = new OrderItem
                            {
                                VariantId = item.VariantId,
                                Quantity = item.Quantity,
                                PriceAtPurchase = variant.Price
                            };
                            orderItems.Add(orderItem);

                            // Calculate total
                            totalAmount += variant.Price * item.Quantity;
                        }

 
                        foreach (var variant in variants)
                        {
                            await _unitOfWork.ProductVariantRepository.UpdateAsync(variant);
                        }
 
                        var order = new Order
                        {
                            UserId = userId,
                            Status = "Pending",
                            TotalAmount = totalAmount,
                            ShippingAddress = dto.ShippingAddress,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.OrderRepository.AddAsync(order);
                        await _unitOfWork.SaveChangesAsync();  
 
                        foreach (var orderItem in orderItems)
                        {
                            orderItem.OrderId = order.OrderId;
                            await _unitOfWork.OrderItemRepository.AddAsync(orderItem);
                        }

                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitAsync();
 
                        var createdOrder = await _unitOfWork.OrderRepository.GetAllQueryable()
                            .Include(o => o.OrderItems)
                                .ThenInclude(oi => oi.Variant)
                                    .ThenInclude(v => v.Product)
                            .AsNoTracking() // Performance optimization
                            .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

                        if (createdOrder == null)
                        {
                            return ServiceResult<OrderDto>.Failure(
                                ApiStatusCodes.InternalServerError,
                                "Order created but failed to retrieve");
                        }

                        var orderDto = _mapper.Map<OrderDto>(createdOrder);

                        _logger.LogInformation(
                            "Order {OrderId} created successfully for user {UserId}",
                            order.OrderId,
                            userId);

                        return ServiceResult<OrderDto>.Success(
                            orderDto,
                            ApiMessages.Order.Created);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in order transaction for user {UserId}", userId);
                        await _unitOfWork.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                return ServiceResult<OrderDto>.Failure(
                    ApiStatusCodes.InternalServerError,
                    "Order creation failed. Please try again.");
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