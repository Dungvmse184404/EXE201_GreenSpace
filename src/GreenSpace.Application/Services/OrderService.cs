using AutoMapper;
using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.Order;
using GreenSpace.Application.Enums;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Constants;
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
        private readonly IStockService _stockService;
        private readonly IPromotionService _promotionService;

        // Default shipping fee (có thể config từ appsettings)
        private const decimal DefaultShippingFee = 30000m;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<OrderService> logger,
            IStockService stockService,
            IPromotionService promotionService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _stockService = stockService;
            _promotionService = promotionService;
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
                return ServiceResult<List<OrderDto>>.Failure(ApiStatusCodes.InternalServerError, ApiMessages.Error.General);
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


        public async Task<IServiceResult<OrderDto>> CreateOrderAsync(
             CreateOrderDto dto,
             Guid userId)
        {
            try
            {
                // check stock availability  
                var stockCheck = await _stockService.CheckStockAvailabilityAsync(dto.Items);
                if (!stockCheck.IsSuccess)
                {
                    return ServiceResult<OrderDto>.Failure(ApiStatusCodes.InternalServerError, stockCheck.Message ?? "Stock unavailable");
                }

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Calculate SubTotal from order items first
                    decimal subTotal = 0;
                    var orderItems = new List<OrderItem>();

                    foreach (var item in dto.Items)
                    {
                        var variant = await _unitOfWork.ProductVariantRepository
                            .GetByIdAsync(item.VariantId);

                        if (variant == null)
                            throw new InvalidOperationException($"Variant {item.VariantId} not found");

                        subTotal += variant.Price * item.Quantity;

                        orderItems.Add(new OrderItem
                        {
                            VariantId = item.VariantId,
                            Quantity = item.Quantity,
                            PriceAtPurchase = variant.Price
                        });
                    }

                    // Calculate discount from voucher
                    decimal discount = 0;
                    string? appliedVoucherCode = null;

                    if (!string.IsNullOrEmpty(dto.VoucherCode))
                    {
                        var discountResult = await _promotionService.ApplyVoucherAsync(dto.VoucherCode, subTotal);
                        if (discountResult.IsSuccess)
                        {
                            discount = discountResult.Data;
                            appliedVoucherCode = dto.VoucherCode.ToUpper();
                            _logger.LogInformation("Voucher {Code} applied. Discount: {Discount}", dto.VoucherCode, discount);
                        }
                        else
                        {
                            _logger.LogWarning("Voucher {Code} failed: {Message}", dto.VoucherCode, discountResult.Message);
                            // Không throw error, chỉ log warning và tiếp tục không có discount
                        }
                    }

                    // Calculate final amount
                    decimal shippingFee = DefaultShippingFee;
                    decimal finalAmount = subTotal - discount + shippingFee;

                    // Create order with all pricing info
                    var order = new Order
                    {
                        UserId = userId,
                        Status = OrderStatus.Pending,
                        ShippingAddress = dto.ShippingAddress,
                        Note = dto.Note,
                        CreatedAt = DateTime.UtcNow,
                        // Price breakdown
                        SubTotal = subTotal,
                        Discount = discount,
                        VoucherCode = appliedVoucherCode,
                        ShippingFee = shippingFee,
                        FinalAmount = finalAmount,
                        TotalAmount = finalAmount // backward compatible
                    };

                    await _unitOfWork.OrderRepository.AddAsync(order);
                    await _unitOfWork.SaveChangesAsync();

                    // Set OrderId for order items and save
                    foreach (var item in orderItems)
                    {
                        item.OrderId = order.OrderId;
                    }

                    await _unitOfWork.OrderItemRepository.AddMultipleAsync(orderItems);
                    await _unitOfWork.SaveChangesAsync();

                    // Reserve stock
                    var reserveResult = await _stockService.ReserveStockAsync(
                        order.OrderId,
                        dto.Items);

                    if (!reserveResult.IsSuccess)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ServiceResult<OrderDto>.Failure(ApiStatusCodes.InternalServerError,
                            reserveResult.Message ?? "Failed to reserve stock");
                    }

                    await _unitOfWork.CommitAsync();

                    // Reload order with items
                    var createdOrder = await _unitOfWork.OrderRepository.GetAllQueryable()
                        .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                        .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

                    var result = _mapper.Map<OrderDto>(createdOrder);

                    _logger.LogInformation(
                        "Order {OrderId} created with stock reserved for user {UserId}",
                        order.OrderId, userId);

                    return ServiceResult<OrderDto>.Success(result, ApiMessages.Order.Created);
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return ServiceResult<OrderDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }


        public async Task<IServiceResult<OrderDto>> UpdateStatusAsync(Guid orderId, string status)
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
                if (order == null)
                    return ServiceResult<OrderDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Order.NotFound);

                var oldStatus = order.Status;
                order.Status = status;

                // Handle stock based on status change
                if (status == OrderStatus.Cancelled)
                {
                    // Revert stock if order cancelled/failed
                    await _stockService.RevertStockReservationAsync(orderId);
                }
                else if (status == OrderStatus.Completed)
                {
                    // Confirm stock deduction
                    await _stockService.ConfirmStockReservationAsync(orderId);
                }

                await _unitOfWork.OrderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<OrderDto>(order);

                _logger.LogInformation(
                    "Order {OrderId} status changed from {OldStatus} to {NewStatus}",
                    orderId, oldStatus, status);

                return ServiceResult<OrderDto>.Success(result, ApiMessages.Order.Updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status {OrderId}", orderId);
                return ServiceResult<OrderDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }
    }
}