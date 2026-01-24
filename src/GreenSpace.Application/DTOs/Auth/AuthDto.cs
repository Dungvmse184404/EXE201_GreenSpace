using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.Auth
{
    public class AuthResultDto
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// User email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// User role name
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// JWT access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration time
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}