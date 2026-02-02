using AutoMapper;
using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.ProductVariant;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductVariantService> _logger;

        public ProductVariantService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ProductVariantService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IServiceResult<List<ProductVariantDto>>> GetByProductIdAsync(Guid productId)
        {
            try
            {
                var variants = await _unitOfWork.ProductVariantRepository.GetAllQueryable()
                    .Where(v => v.ProductId == productId && v.IsActive == true)
                    .ToListAsync();

                var dtos = _mapper.Map<List<ProductVariantDto>>(variants);
                return ServiceResult<List<ProductVariantDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting variants for product {ProductId}", productId);
                return ServiceResult<List<ProductVariantDto>>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<ProductVariantDto>> GetByIdAsync(Guid variantId)
        {
            try
            {
                var variant = await _unitOfWork.ProductVariantRepository.GetByIdAsync(variantId);

                if (variant == null || variant.IsActive != true)
                    return ServiceResult<ProductVariantDto>.Failure( ApiStatusCodes.NotFound, ApiMessages.ProductVariant.NotFound);

                var dto = _mapper.Map<ProductVariantDto>(variant);
                return ServiceResult<ProductVariantDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting variant {VariantId}", variantId);
                return ServiceResult<ProductVariantDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<ProductVariantDto>> CreateAsync(CreateProductVariantDto dto)
        {
            try
            {
                // Validate product exists
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(dto.ProductId);
                if (product == null || product.IsActive != true)
                    return ServiceResult<ProductVariantDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.ProductVariant.NotFound);

                // Check SKU uniqueness
                var existingSku = await _unitOfWork.ProductVariantRepository.GetAllQueryable()
                    .AnyAsync(v => v.Sku == dto.Sku);

                if (existingSku)
                    return ServiceResult<ProductVariantDto>.Failure(ApiStatusCodes.Conflict, ApiMessages.ProductVariant.SkuExists);

                var variant = _mapper.Map<ProductVariant>(dto);
                variant.IsActive = true;
                variant.CreatedAt = DateTime.UtcNow;
                variant.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductVariantRepository.AddAsync(variant);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<ProductVariantDto>(variant);
                return ServiceResult<ProductVariantDto>.Success(result, ApiMessages.ProductVariant.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating variant");
                return ServiceResult<ProductVariantDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<ProductVariantDto>> UpdateAsync(
            Guid variantId,
            UpdateProductVariantDto dto)
        {
            try
            {
                var variant = await _unitOfWork.ProductVariantRepository.GetByIdAsync(variantId);
                if (variant == null)
                    return ServiceResult<ProductVariantDto>.Failure( ApiStatusCodes.NotFound, ApiMessages.ProductVariant.NotFound);

                var existingSku = await _unitOfWork.ProductVariantRepository.GetAllQueryable()
                    .AnyAsync(v => v.Sku == dto.Sku && v.VariantId != variantId);

                if (existingSku)
                    return ServiceResult<ProductVariantDto>.Failure(ApiStatusCodes.Conflict, ApiMessages.ProductVariant.SkuExists);

                _mapper.Map(dto, variant);
                variant.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductVariantRepository.UpdateAsync(variant);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<ProductVariantDto>(variant);
                return ServiceResult<ProductVariantDto>.Success(result, ApiMessages.ProductVariant.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating variant {VariantId}", variantId);
                return ServiceResult<ProductVariantDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<bool>> DeleteAsync(Guid variantId)
        {
            try
            {
                var variant = await _unitOfWork.ProductVariantRepository.GetByIdAsync(variantId);
                if (variant == null)
                    return ServiceResult<bool>.Failure(ApiStatusCodes.NotFound, ApiMessages.ProductVariant.NotFound);

                // Soft delete
                variant.IsActive = false;
                variant.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductVariantRepository.UpdateAsync(variant);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, ApiMessages.ProductVariant.Deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting variant {VariantId}", variantId);
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<bool>> UpdateStockAsync(Guid variantId, int quantity)
        {
            try
            {
                var variant = await _unitOfWork.ProductVariantRepository.GetByIdAsync(variantId);
                if (variant == null || variant.IsActive != true)
                    return ServiceResult<bool>.Failure(ApiStatusCodes.NotFound, ApiMessages.ProductVariant.NotFound);

                variant.StockQuantity = quantity;
                variant.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductVariantRepository.UpdateAsync(variant);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, ApiMessages.ProductVariant.StockUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for variant {VariantId}", variantId);
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }
    }
}