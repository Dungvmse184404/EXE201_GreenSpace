using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Domain.Models;
using GreenSpace.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Infrastructure.Persistence.Repositories;

public class UserAddressRepository : GenericRepository<UserAddress>, IUserAddressRepository
{
    public UserAddressRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserAddress?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.UserAddresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault, cancellationToken);
    }

    public async Task<UserAddress?> GetByIdAndUserIdAsync(Guid addressId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.UserAddresses
            .FirstOrDefaultAsync(a => a.AddressId == addressId && a.UserId == userId, cancellationToken);
    }

    public async Task ResetDefaultAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _dbSet.UserAddresses
            .Where(a => a.UserId == userId && a.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), cancellationToken);
    }
}
