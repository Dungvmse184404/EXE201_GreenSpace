using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Infrastructure.Persistence.Contexts;
using GreenSpace.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        private IDbContextTransaction? _transaction;

        // Khai báo Lazy cho tất cả Repository
        private readonly Lazy<IAttributeRepository> _attributeRepository;
        private readonly Lazy<ICategoryRepository> _categoryRepository;
        private readonly Lazy<IOrderItemRepository> _orderItemRepository;
        private readonly Lazy<IOrderRepository> _orderRepository;
        private readonly Lazy<IProductAttributeValueRepository> _productAttributeValueRepository;
        private readonly Lazy<IProductRepository> _productRepository;
        private readonly Lazy<IProductVariantRepository> _productVariantRepository;
        private readonly Lazy<IPromotionRepository> _promotionRepository;
        private readonly Lazy<IRatingRepository> _ratingRepository;
        private readonly Lazy<IUserRepository> _userRepository;
        private readonly Lazy<IUserAddressRepository> _userAddressRepository;
        private readonly Lazy<IPaymentRepository> _paymentRepository;
        private readonly Lazy<ICartRepository> _cartRepository;
        private readonly Lazy<ICartItemRepository> _cartItemRepository;

        public UnitOfWork(AppDbContext context, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _context = context;
            _configuration = configuration;
            _loggerFactory = loggerFactory;

            // Khởi tạo Lazy: Chỉ new khi cần dùng (.Value)
            _attributeRepository = new Lazy<IAttributeRepository>(() => new AttributeRepository(_context));
            _categoryRepository = new Lazy<ICategoryRepository>(() => new CategoryRepository(_context));
            _orderItemRepository = new Lazy<IOrderItemRepository>(() => new OrderItemRepository(_context));
            _orderRepository = new Lazy<IOrderRepository>(() => new OrderRepository(_context));
            _productAttributeValueRepository = new Lazy<IProductAttributeValueRepository>(() => new ProductAttributeValueRepository(_context));
            _productRepository = new Lazy<IProductRepository>(() => new ProductRepository(_context));
            _productVariantRepository = new Lazy<IProductVariantRepository>(() => new ProductVariantRepository(_context));
            _promotionRepository = new Lazy<IPromotionRepository>(() => new PromotionRepository(_context));
            _ratingRepository = new Lazy<IRatingRepository>(() => new RatingRepository(_context));
            _userRepository = new Lazy<IUserRepository>(() => new UserRepository(_context));
            _userAddressRepository = new Lazy<IUserAddressRepository>(() => new UserAddressRepository(_context));
            _paymentRepository = new Lazy<IPaymentRepository>(() => new PaymentRepository(_context));
            _cartRepository = new Lazy<ICartRepository>(() => new CartRepository(_context));
            _cartItemRepository = new Lazy<ICartItemRepository>(() => new CartItemRepository(_context));
        }

        public IAttributeRepository AttributeRepository => _attributeRepository.Value;
        public ICategoryRepository CategoryRepository => _categoryRepository.Value;
        public IOrderItemRepository OrderItemRepository => _orderItemRepository.Value;
        public IOrderRepository OrderRepository => _orderRepository.Value;
        public IProductAttributeValueRepository ProductAttributeValueRepository => _productAttributeValueRepository.Value;
        public IProductRepository ProductRepository => _productRepository.Value;
        public IProductVariantRepository ProductVariantRepository => _productVariantRepository.Value;
        public IPromotionRepository PromotionRepository => _promotionRepository.Value;
        public IRatingRepository RatingRepository => _ratingRepository.Value;
        public IUserRepository UserRepository => _userRepository.Value;
        public IUserAddressRepository UserAddressRepository => _userAddressRepository.Value;
        public IPaymentRepository PaymentRepository => _paymentRepository.Value;
        public ICartRepository CartRepository => _cartRepository.Value;
        public ICartItemRepository CartItemRepository => _cartItemRepository.Value;


        // --- Xử lý Transaction ---
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null) return;
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await SaveChangesAsync(cancellationToken); // Lưu thay đổi trước khi commit
                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // --- Xử lý Save Changes ---
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        // --- Xử lý Dispose (Dọn dẹp bộ nhớ) ---
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }
            await _context.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}