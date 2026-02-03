using GreenSpace.Application.DTOs.Order;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IStockService
    {
        /// <summary>
        /// Reserve stock for order items (deduct stock)
        /// </summary>
        Task<IServiceResult<bool>> ReserveStockAsync(
            Guid orderId,
            List<CreateOrderItemDto> items,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirm stock reservation (when payment succeeds)
        /// </summary>
        Task<IServiceResult<bool>> ConfirmStockReservationAsync(
            Guid orderId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Revert stock reservation (when payment fails/expires)
        /// </summary>
        Task<IServiceResult<bool>> RevertStockReservationAsync(
            Guid orderId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if sufficient stock is available
        /// </summary>
        Task<IServiceResult<bool>> CheckStockAvailabilityAsync(
            List<CreateOrderItemDto> items,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cleanup expired reservations (background job)
        /// </summary>
        Task<IServiceResult<int>> CleanupExpiredReservationsAsync(
            CancellationToken cancellationToken = default);
    }
}
