using GreenSpace.Application.DTOs.ProductVariant;
using GreenSpace.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IProductVariantService
    {
        Task<IServiceResult<List<ProductVariantDto>>> GetByProductIdAsync(Guid productId);
        Task<IServiceResult<ProductVariantDto>> GetByIdAsync(Guid variantId);
        Task<IServiceResult<ProductVariantDto>> CreateAsync(CreateProductVariantDto dto);
        Task<IServiceResult<ProductVariantDto>> UpdateAsync(Guid variantId, UpdateProductVariantDto dto);
        Task<IServiceResult<bool>> DeleteAsync(Guid variantId);
        Task<IServiceResult<bool>> UpdateStockAsync(Guid variantId, int quantity);
    }
}

