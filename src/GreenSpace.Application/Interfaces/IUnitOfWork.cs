using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Application.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GreenSpace.Application.Interfaces
{
    /// <summary>
    /// Represents a unit of work that coordinates persistence operations and transactions.
    /// Implementations should handle underlying context lifecycle and transaction management.
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        IAttributeRepository AttributeRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IOrderItemRepository OrderItemRepository { get; }
        IOrderRepository OrderRepository { get; }
        IProductVariantRepository ProductVariantRepository { get; }
        IProductAttributeValueRepository ProductAttributeValueRepository { get; }
        IProductRepository ProductRepository { get; }
        IPromotionRepository PromotionRepository { get; }
        IRatingRepository RatingRepository { get; }
        IUserRepository UserRepository { get; }
        IUserAddressRepository UserAddressRepository { get; }
        IPaymentRepository PaymentRepository { get; }
        ICartRepository CartRepository { get; }
        ICartItemRepository CartItemRepository { get; }




        IRefreshTokenRepository RefreshTokenRepository { get; }
        IRefreshTokenService RefreshTokenService { get; }



        /// <summary>
        /// Persists all changes synchronously.
        /// </summary>
        /// <returns>The number of state entries written to the underlying store.</returns>
        int SaveChanges();

        /// <summary>
        /// Persists all changes asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The number of state entries written to the underlying store.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a new transaction scope asynchronously.
        /// Implementations may be no-op if the underlying provider manages transactions differently.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the current transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the current transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
