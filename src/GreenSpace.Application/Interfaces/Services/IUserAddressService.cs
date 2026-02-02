using GreenSpace.Application.DTOs.User;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IUserAddressService
    {
        Task<IServiceResult<List<UserAddressDto>>> GetByUserIdAsync(Guid userId);
        Task<IServiceResult<UserAddressDto>> GetByIdAsync(Guid addressId);
        Task<IServiceResult<UserAddressDto>> CreateAsync(CreateUserAddressDto dto, Guid userId);
        Task<IServiceResult<UserAddressDto>> UpdateAsync(Guid addressId, UpdateUserAddressDto dto, Guid userId);
        Task<IServiceResult<bool>> DeleteAsync(Guid addressId, Guid userId);
    }
}