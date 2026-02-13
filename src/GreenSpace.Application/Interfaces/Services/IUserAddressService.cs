using GreenSpace.Application.DTOs.UserAddress;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Services;

public interface IUserAddressService
{
    /// <summary>
    /// Lay tat ca dia chi cua user
    /// </summary>
    Task<IServiceResult> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Lay dia chi theo ID
    /// </summary>
    Task<IServiceResult> GetByIdAsync(Guid addressId, Guid userId);

    /// <summary>
    /// Lay dia chi mac dinh cua user
    /// </summary>
    Task<IServiceResult> GetDefaultAsync(Guid userId);

    /// <summary>
    /// Tao dia chi moi
    /// </summary>
    Task<IServiceResult> CreateAsync(CreateUserAddressDto dto, Guid userId);

    /// <summary>
    /// Cap nhat dia chi
    /// </summary>
    Task<IServiceResult> UpdateAsync(Guid addressId, UpdateUserAddressDto dto, Guid userId);

    /// <summary>
    /// Xoa dia chi
    /// </summary>
    Task<IServiceResult> DeleteAsync(Guid addressId, Guid userId);

    /// <summary>
    /// Dat dia chi lam mac dinh
    /// </summary>
    Task<IServiceResult> SetDefaultAsync(Guid addressId, Guid userId);
}