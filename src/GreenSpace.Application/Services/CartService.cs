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

        public async Task<IServiceResult<CartDto>> AddItemAsync(Guid userId, ModifyCartItemDto dto)
        {
            try
            {
                // Validate Variant Existence & Stock
                var variant = await _unitOfWork.ProductVariantRepository.GetByIdAsync(dto.VariantId);
                if (variant == null)
                {
                    return ServiceResult<CartDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.ProductVariant.NotFound);
                }

                if (variant.StockQuantity < dto.Quantity)
                {
                    return ServiceResult<CartDto>.Failure(ApiStatusCodes.BadRequest, ApiMessages.ProductVariant.InsufficientStock);
                }

                // Get or Create Cart
                var cart = await GetCartWithItemsAsync(userId);
                if (cart == null)
                {
                    cart = await CreateNewCartAsync(userId);
                }

                // Update or Add Item
                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.VariantId == dto.VariantId);
                if (existingItem != null)
                {
                    existingItem.Quantity += dto.Quantity;
                    // Optional: Check stock again against total quantity
                    if (variant.StockQuantity < existingItem.Quantity)
                        return ServiceResult<CartDto>.Failure(ApiStatusCodes.BadRequest,ApiMessages.ProductVariant.InsufficientStock);
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

                return ServiceResult<CartDto>.Success(_mapper.Map<CartDto>(cart), ApiMessages.ProductVariant.Added);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for user {UserId}", userId);
                return ServiceResult<CartDto>.Failure(ApiStatusCodes.InternalServerError, ApiMessages.ProductVariant.AddFailed);
            }
        }

        public async Task<IServiceResult<CartDto>> RemoveItemAsync(Guid userId, ModifyCartItemDto dto)
        {
            try
            {
                var cart = await GetCartWithItemsAsync(userId);
                if (cart == null) return ServiceResult<CartDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Cart.NotFound);

                var item = cart.CartItems.FirstOrDefault(ci => ci.VariantId == dto.VariantId);
                if (item == null) return ServiceResult<CartDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Cart.ItemNotFound);

                // Caculate numbers
                item.Quantity -= dto.Quantity;

                if (item.Quantity <= 0)
                {
                    // <= 0 -> delete
                    await _unitOfWork.CartItemRepository.RemoveAsync(item);
                }
                else
                {
                    // > 0 -> update 
                    await _unitOfWork.CartItemRepository.UpdateAsync(item);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<CartDto>.Success(_mapper.Map<CartDto>(cart), ApiMessages.Cart.Updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error modifying item {VariantId} for user {UserId}", dto.VariantId, userId);
                return ServiceResult<CartDto>.Failure(ApiStatusCodes.InternalServerError, ApiMessages.Cart.UpdateFailed);
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

                return ServiceResult<bool>.Success(true, ApiMessages.Cart.Cleared);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, ApiMessages.Cart.ClearFailed);
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Gets the cart with items.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        private async Task<Cart?> GetCartWithItemsAsync(Guid userId)
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