using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.Common.Settings;
using GreenSpace.Application.DTOs.Order;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Constants;
using GreenSpace.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Services
{
    public class StockService : IStockService
    {
        private readonly VNPaySettings _settings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StockService> _logger;
        private readonly int _paymentExpiryMinutes;
        private readonly int _maxRetryAttempts = 3;


        public StockService(
            IOptions<VNPaySettings> settings,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<StockService> logger)
        {
            _settings = settings.Value;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _paymentExpiryMinutes = settings.Value.TimeoutMinutes;
        }

        public async Task<IServiceResult<bool>> ReserveStockAsync(
            Guid orderId,
            List<CreateOrderItemDto> items,
            CancellationToken cancellationToken = default)
        {
            var attempt = 0;

            while (attempt < _maxRetryAttempts)
            {
                try
                {
                    attempt++;
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);

                    // 1. Check stock availability first
                    foreach (var item in items)
                    {
                        var variant = await _unitOfWork.ProductVariantRepository
                            .GetByIdAsync(item.VariantId);

                        if (variant == null || variant.IsActive != true)
                        {
                            await _unitOfWork.RollbackAsync(cancellationToken);
                            return ServiceResult<bool>.Failure(ApiStatusCodes.NotFound, $"Product variant {item.VariantId} not found");
                        }

                        if (variant.StockQuantity < item.Quantity)
                        {
                            await _unitOfWork.RollbackAsync(cancellationToken);
                            return ServiceResult<bool>.Failure(ApiStatusCodes.Conflict,
                                $"Insufficient stock for {variant.Sku}. Available: {variant.StockQuantity}, Requested: {item.Quantity}");
                        }
                    }

                    // 2. Deduct stock with optimistic concurrency control
                    foreach (var item in items)
                    {
                        var variant = await _unitOfWork.ProductVariantRepository
                            .GetByIdAsync(item.VariantId);

                        // Deduct stock
                        variant!.StockQuantity -= item.Quantity;
                        variant.UpdatedAt = DateTime.UtcNow;

                        await _unitOfWork.ProductVariantRepository.UpdateAsync(variant);
                    }

                    // 3. Mark order as stock reserved
                    var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
                    if (order != null)
                    {
                        order.StockReserved = true;
                        order.PaymentExpiryAt = DateTime.UtcNow.AddMinutes(_paymentExpiryMinutes);
                        await _unitOfWork.OrderRepository.UpdateAsync(order);
                    }

                    // 4. Commit transaction
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "Stock reserved successfully for order {OrderId} (Attempt {Attempt})",
                        orderId, attempt);

                    return ServiceResult<bool>.Success(true, "Stock reserved successfully");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);

                    _logger.LogWarning(
                        "Concurrency conflict on attempt {Attempt} for order {OrderId}: {Message}",
                        attempt, orderId, ex.Message);

                    if (attempt >= _maxRetryAttempts)
                    {
                        _logger.LogError(
                            "Failed to reserve stock after {MaxAttempts} attempts for order {OrderId}",
                            _maxRetryAttempts, orderId);

                        return ServiceResult<bool>.Failure(ApiStatusCodes.Conflict, ApiMessages.Stock.HighTraffic
                            );
                    }

                    // Wait before retry (exponential backoff)
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error reserving stock for order {OrderId}", orderId);
                    return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
                }
            }

            return ServiceResult<bool>.Failure(ApiStatusCodes.Conflict, ApiMessages.Stock.MutipleAttempts);
        }

        public async Task<IServiceResult<bool>> ConfirmStockReservationAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return ServiceResult<bool>.Failure(ApiStatusCodes.NotFound, ApiMessages.Order.NotFound);
                }

                if (!order.StockReserved)
                {
                    _logger.LogWarning("Order {OrderId} has no stock reservation", orderId);
                    return ServiceResult<bool>.Success(true, ApiMessages.Stock.NoStock);
                }

                // Just mark as confirmed (stock already deducted)
                order.PaymentExpiryAt = null; // Clear expiry
                await _unitOfWork.OrderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Stock reservation confirmed for order {OrderId}", orderId);
                return ServiceResult<bool>.Success(true, ApiMessages.Stock.Confirmed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming stock for order {OrderId}", orderId);
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<bool>> RevertStockReservationAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var order = await _unitOfWork.OrderRepository.GetAllQueryable()
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

                if (order == null)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return ServiceResult<bool>.Failure(ApiStatusCodes.NotFound, ApiMessages.Order.NotFound);
                }

                if (!order.StockReserved)
                {
                    _logger.LogInformation("Order {OrderId} has no stock to revert", orderId);
                    return ServiceResult<bool>.Success(true, ApiMessages.Stock.NoStock);
                }

                // Add stock back
                foreach (var item in order.OrderItems)
                {
                    if (item.Variant != null)
                    {
                        var variant = await _unitOfWork.ProductVariantRepository
                            .GetByIdAsync(item.Variant.VariantId);

                        if (variant != null)
                        {
                            variant.StockQuantity = (variant.StockQuantity ?? 0) + item.Quantity;
                            variant.UpdatedAt = DateTime.UtcNow;
                            await _unitOfWork.ProductVariantRepository.UpdateAsync(variant);
                        }
                    }
                }

                // Update order
                order.StockReserved = false;
                order.PaymentExpiryAt = null;
                await _unitOfWork.OrderRepository.UpdateAsync(order);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Stock reverted successfully for order {OrderId}", orderId);
                return ServiceResult<bool>.Success(true, ApiMessages.Stock.Released);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error reverting stock for order {OrderId}", orderId);
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<bool>> CheckStockAvailabilityAsync(
            List<CreateOrderItemDto> items,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var unavailableItems = new List<string>();

                foreach (var item in items)
                {
                    var variant = await _unitOfWork.ProductVariantRepository
                        .GetByIdAsync(item.VariantId);

                    if (variant == null || variant.IsActive != true)
                    {
                        unavailableItems.Add($"Variant {item.VariantId} not found");
                        continue;
                    }

                    if (variant.StockQuantity < item.Quantity)
                    {
                        unavailableItems.Add(
                            $"{variant.Sku}: Available {variant.StockQuantity}, Requested {item.Quantity}");
                    }
                }

                if (unavailableItems.Any())
                {
                    return ServiceResult<bool>.Failure(ApiStatusCodes.Conflict,
                        $"Insufficient stock: {string.Join("; ", unavailableItems)}");
                }

                return ServiceResult<bool>.Success(true, "Stock available");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock availability");
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<int>> CleanupExpiredReservationsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var expiredOrders = await _unitOfWork.OrderRepository.GetAllQueryable()
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                    .Where(o => o.StockReserved
                        && o.PaymentExpiryAt.HasValue
                        && o.PaymentExpiryAt.Value <= DateTime.UtcNow
                        && o.Status == OrderStatus.Pending)
                    .ToListAsync(cancellationToken);

                var revertedCount = 0;

                foreach (var order in expiredOrders)
                {
                    var result = await RevertStockReservationAsync(order.OrderId, cancellationToken);
                    if (result.IsSuccess)
                    {
                        // Update order status
                        order.Status = OrderStatus.Cancelled;
                        await _unitOfWork.OrderRepository.UpdateAsync(order);
                        revertedCount++;
                    }
                }

                if (revertedCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation(
                    "Cleaned up {Count} expired stock reservations",
                    revertedCount);

                return ServiceResult<int>.Success(
                    revertedCount,
                    $"Reverted {revertedCount} expired reservations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired reservations");
                return ServiceResult<int>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }
    }

}
