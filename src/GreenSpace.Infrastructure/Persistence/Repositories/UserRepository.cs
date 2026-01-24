using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Domain.Models;
using GreenSpace.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Infrastructure.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get user by email address
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User entity if found</returns>
        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Users
                .Where(u => u.Email.ToLower() == email.ToLower() && u.IsActive == true)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
        }

        /// <summary>
        /// Get user with role information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User with role</returns>
        public async Task<User?> GetUserWithRoleAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Users
                .Include(u => u.Role)
                .Where(u => u.UserId == userId && u.IsActive == true)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Get user by ID with role information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User with role</returns>
        public async Task<User?> GetByIdWithRoleAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Users
                .Include(u => u.Role)
                .Where(u => u.UserId == userId && u.IsActive == true)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Get user by email with role information
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User with role</returns>
        public async Task<User?> GetByEmailWithRoleAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Users
                .Include(u => u.Role)
                .Where(u => u.Email.ToLower() == email.ToLower() && u.IsActive == true)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> PhoneExistsAsync(string phone, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Users
                   .AnyAsync(u => u.Phone == phone.ToLower(), cancellationToken);
        }
    }
}
