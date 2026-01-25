using AutoMapper;
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
                var cart = await _unitOfWork.CartRepository.GetAllQueryable()
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Variant)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.CartRepository.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                }

                var dto = _mapper.Map<CartDto>(cart);
                return ServiceResult<CartDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for user {UserId}", userId);
                return ServiceResult<CartDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<CartDto>> AddItemAsync(Guid userId, AddCartItemDto dto)
        {
            try
            {
                var cart = await _unitOfWork.CartRepository.GetAllQueryable()
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
                    await _unitOfWork.CartRepository.AddAsync(cart);
                }

                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.VariantId == dto.VariantId);
                if (existingItem != null)
                {
                    existingItem.Quantity += dto.Quantity;
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

                var result = _mapper.Map<CartDto>(cart);
                return ServiceResult<CartDto>.Success(result, "Item added to cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return ServiceResult<CartDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<CartDto>> RemoveItemAsync(Guid userId, Guid cartItemId)
        {
            try
            {
                var cart = await _unitOfWork.CartRepository.GetAllQueryable()
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                    return ServiceResult<CartDto>.Failure("Cart not found");

                var item = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
                if (item == null)
                    return ServiceResult<CartDto>.Failure("Item not found in cart");

                await _unitOfWork.CartItemRepository.RemoveAsync(item);
                cart.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<CartDto>(cart);
                return ServiceResult<CartDto>.Success(result, "Item removed from cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                return ServiceResult<CartDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<bool>> ClearCartAsync(Guid userId)
        {
            try
            {
                var cart = await _unitOfWork.CartRepository.GetAllQueryable()
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                    return ServiceResult<bool>.Success(true, "Cart already empty");

                await _unitOfWork.CartItemRepository.RemoveMultipleEntitiesAsync(cart.CartItems.ToList());
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, "Cart cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return ServiceResult<bool>.Failure($"Error: {ex.Message}");
            }
        }
    }
}