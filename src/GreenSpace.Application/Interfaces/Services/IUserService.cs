using GreenSpace.Application.DTOs.User;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<IServiceResult<UserDto>> GetByIdAsync(Guid userId);
        Task<IServiceResult<List<UserDto>>> GetAllAsync();
        Task<IServiceResult<UserDto>> UpdateAsync(Guid userId, UpdateUserDto dto);
        Task<IServiceResult<bool>> DeactivateAsync(Guid userId);
    }
}
