using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.User
{
    public class UserDto
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
        public string FullName { get; set; }

        /// <summary>
        /// User phone number
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// User address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// User birthday
        /// </summary>
        public DateOnly? Birthday { get; set; }

        /// <summary>
        /// User role name
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// User status
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// User active status
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Last update date
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
