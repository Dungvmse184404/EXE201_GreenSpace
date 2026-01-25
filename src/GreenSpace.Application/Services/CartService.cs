using AutoMapper;
using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.Cart;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CartService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IServiceResult<CartDto>> GetUserCartAsync(Guid userId)
        {
            try
            {
                var cart = await GetCartWithItemsAsync(userId);

                if (cart == null)
                {
                    cart = await CreateNewCartAsync(userId);
                }

                return ServiceResult<CartDto>.Success(_mapper.Map<CartDto>(cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}", userId);
                return ServiceResult<CartDto>.Failure(ApiStatusCodes.InternalServerError, "Could not retrieve cart.");
            }
        }

        public async Task<IServiceResult<CartDto>> AddItemAsync(Guid userId, AddCartItemDto dto)
        {
            try
            {
                // 1. Validate Variant Existence & Stock
                var variant = await _unitOfWork.ProductVariantRepository.GetByIdAsync(dto.VariantId);
                if (variant == null)
                {
                    return ServiceResult<CartDto>.Failure(ApiStatusCodes.NotFound, "Product variant not found.");
                }

                if (variant.StockQuantity < dto.Quantity)
                {
                    return ServiceResult<CartDto>.Failure(ApiStatusCodes.BadRequest, "Not enough stock available.");
                }

                // 2. Get or Create Cart
                var cart = await GetCartWithItemsAsync(userId);
                if (cart == null)
                {
                    cart = await CreateNewCartAsync(userId);
                }

                // 3. Update or Add Item
                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.VariantId == dto.VariantId);
                if (existingItem != null)
                {
                    existingItem.Quantity += dto.Quantity;
                    // Optional: Check stock again against total quantity
                    if (variant.StockQuantity < existingItem.Quantity)
                        return ServiceResult<CartDto>.Failure(ApiStatusCodes.BadRequest, "Total quantity exceeds available stock.");
                }
                else
                {
                    cart.CartItems.Add(new CartItem
                    {
                        VariantId = dto.VariantId,
                        Quantity = dto.Quantity,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                cart.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<CartDto>.Success(_mapper.Map<CartDto>(cart), "Item added to cart successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for user {UserId}", userId);
                return ServiceResult<CartDto>.Failure(ApiStatusCodes.InternalServerError, "Failed to add item to cart.");
            }
        }

        public async Task<IServiceResult<CartDto>> RemoveItemAsync(Guid userId, Guid cartItemId)
        {
            try
            {
                var cart = await GetCartWithItemsAsync(userId);
                if (cart == null) return ServiceResult<CartDto>.Failure(ApiStatusCodes.NotFound, "Cart not found.");

                var item = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
                if (item == null) return ServiceResult<CartDto>.Failure(ApiStatusCodes.NotFound, "Item not found in cart.");

                await _unitOfWork.CartItemRepository.RemoveAsync(item);
                cart.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<CartDto>.Success(_mapper.Map<CartDto>(cart), "Item removed from cart.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item {CartItemId} for user {UserId}", cartItemId, userId);
                return ServiceResult<CartDto>.Failure(ApiStatusCodes.InternalServerError, "Failed to remove item.");
            }
        }

        public async Task<IServiceResult<bool>> ClearCartAsync(Guid userId)
        {
            try
            {
                var cart = await _unitOfWork.CartRepository.GetAllQueryable()
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                    return ServiceResult<bool>.Success(true, "Cart is already empty.");

                await _unitOfWork.CartItemRepository.RemoveMultipleEntitiesAsync(cart.CartItems.ToList());
                cart.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, "Cart cleared successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, "Failed to clear cart.");
            }
        }

        #region Private Helper Methods

        private async Task<Cart> GetCartWithItemsAsync(Guid userId)
        {
            return await _unitOfWork.CartRepository.GetAllQueryable()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                        .ThenInclude(v => v.Product)  
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        private async Task<Cart> CreateNewCartAsync(Guid userId)
        {
            var cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CartItems = new List<CartItem>()
            };
            await _unitOfWork.CartRepository.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
            return cart;
        }

        #endregion
    }
}