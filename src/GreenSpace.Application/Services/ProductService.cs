using AutoMapper;
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
                    .Include(p => p.Category)
                    .Include(p => p.ProductVariants)
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive == true);

                if (product == null)
                    return ServiceResult<ProductDto>.Failure("Product not found");

                var dto = _mapper.Map<ProductDto>(product);
                return ServiceResult<ProductDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", productId);
                return ServiceResult<ProductDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<List<ProductDto>>> GetAllAsync()
        {
            try
            {
                var products = await _unitOfWork.ProductRepository.GetAllQueryable()
                    .Include(p => p.Category)
                    .Include(p => p.ProductVariants)
                    .Where(p => p.IsActive == true)
                    .ToListAsync();

                var dtos = _mapper.Map<List<ProductDto>>(products);
                return ServiceResult<List<ProductDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return ServiceResult<List<ProductDto>>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<ProductDto>> CreateAsync(CreateProductDto dto)
        {
            try
            {
                var product = _mapper.Map<Product>(dto);
                product.IsActive = true;
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductRepository.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<ProductDto>(product);
                return ServiceResult<ProductDto>.Success(result, "Product created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return ServiceResult<ProductDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<ProductDto>> UpdateAsync(Guid productId, UpdateProductDto dto)
        {
            try
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId);
                if (product == null || product.IsActive != true)
                    return ServiceResult<ProductDto>.Failure("Product not found");

                _mapper.Map(dto, product);
                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductRepository.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<ProductDto>(product);
                return ServiceResult<ProductDto>.Success(result, "Product updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", productId);
                return ServiceResult<ProductDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<bool>> DeleteAsync(Guid productId)
        {
            try
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId);
                if (product == null)
                    return ServiceResult<bool>.Failure("Product not found");

                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductRepository.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, "Product deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", productId);
                return ServiceResult<bool>.Failure($"Error: {ex.Message}");
            }
        }
    }
}