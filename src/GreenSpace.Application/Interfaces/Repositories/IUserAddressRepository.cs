using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Interfaces.Repositories;

public interface IUserAddressRepository : IGenericRepository<UserAddress>
{
    /// <summary>
    /// Lấy tất cả địa chỉ của user
    /// </summary>
    Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy địa chỉ mặc định của user
    /// </summary>
    Task<UserAddress?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy địa chỉ cụ thể của user (đảm bảo ownership)
    /// </summary>
    Task<UserAddress?> GetByIdAndUserIdAsync(Guid addressId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset tất cả địa chỉ của user về IsDefault = false
    /// </summary>
    Task ResetDefaultAsync(Guid userId, CancellationToken cancellationToken = default);
}
