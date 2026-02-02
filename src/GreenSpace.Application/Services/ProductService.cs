using AutoMapper;
using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.Product;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IServiceResult<ProductDto>> GetByIdAsync(Guid productId)
        {
            try
            {
                var product = await _unitOfWork.ProductRepository.GetAllQueryable()
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.ProductVariants)
                    // .Include(p => p.ProductImages) 
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive == true);

                if (product == null)
                    return ServiceResult<ProductDto>.Failure(ApiStatusCodes.NotFound, "Product not found or inactive.");

                return ServiceResult<ProductDto>.Success(_mapper.Map<ProductDto>(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", productId);
                return ServiceResult<ProductDto>.Failure(ApiStatusCodes.InternalServerError, "Error fetching product details.");
            }
        }

        public async Task<IServiceResult<List<ProductDto>>> GetAllAsync()
        {
            try
            {
                var products = await _unitOfWork.ProductRepository.GetAllQueryable()
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.ProductVariants)
                    .Where(p => p.IsActive == true)
                    .ToListAsync();

                return ServiceResult<List<ProductDto>>.Success(_mapper.Map<List<ProductDto>>(products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return ServiceResult<List<ProductDto>>.Failure(ApiStatusCodes.InternalServerError, "Could not retrieve product list.");
            }
        }

        public async Task<IServiceResult<ProductDto>> CreateAsync(CreateProductDto dto)
        {
            try
            {
                // 1. Validate Category Existence
                var categoryExists = await _unitOfWork.CategoryRepository.GetByIdAsync(dto.CategoryId);
                if (categoryExists == null)
                    return ServiceResult<ProductDto>.Failure(ApiStatusCodes.BadRequest, "Specified category does not exist.");

                var product = _mapper.Map<Product>(dto);
                product.IsActive = true;
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductRepository.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<ProductDto>.Success(_mapper.Map<ProductDto>(product), "Product created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {Name}", dto.Name);
                return ServiceResult<ProductDto>.Failure(ApiStatusCodes.InternalServerError, "Failed to create product.");
            }
        }

        public async Task<IServiceResult<ProductDto>> UpdateAsync(Guid productId, UpdateProductDto dto)
        {
            try
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId);

                if (product == null || product.IsActive != true)
                    return ServiceResult<ProductDto>.Failure(ApiStatusCodes.NotFound, "Product not found or is inactive.");

                _mapper.Map(dto, product);
                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductRepository.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<ProductDto>.Success(_mapper.Map<ProductDto>(product), "Product updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", productId);
                return ServiceResult<ProductDto>.Failure(ApiStatusCodes.InternalServerError, "Failed to update product.");
            }
        }

        public async Task<IServiceResult<bool>> DeleteAsync(Guid productId)
        {
            try
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId);
                if (product == null)
                    return ServiceResult<bool>.Failure(ApiStatusCodes.NotFound, "Product not found.");

                // Soft Delete: Keep the data but hide it from the UI
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductRepository.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, "Product marked as inactive.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting product {ProductId}", productId);
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, "Failed to delete product.");
            }
        }
    }
} 