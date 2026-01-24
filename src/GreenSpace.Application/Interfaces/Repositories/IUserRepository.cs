using GreenSpace.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        /// <summary>
        /// Get user by email address
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User entity if found</returns>
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if email already exists
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if email exists</returns>
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

        Task<bool> PhoneExistsAsync(string phone, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get user with role information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User with role</returns>
        Task<User?> GetUserWithRoleAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user by ID with role information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User with role</returns>
        Task<User?> GetByIdWithRoleAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user by email with role information
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User with role</returns>
        Task<User?> GetByEmailWithRoleAsync(string email, CancellationToken cancellationToken = default);
    }
}
