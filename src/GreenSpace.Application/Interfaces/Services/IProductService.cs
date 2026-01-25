using GreenSpace.Application.DTOs.Product;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<IServiceResult<ProductDto>> GetByIdAsync(Guid productId);
        Task<IServiceResult<List<ProductDto>>> GetAllAsync();
        Task<IServiceResult<ProductDto>> CreateAsync(CreateProductDto dto);
        Task<IServiceResult<ProductDto>> UpdateAsync(Guid productId, UpdateProductDto dto);
        Task<IServiceResult<bool>> DeleteAsync(Guid productId);
    }
}
