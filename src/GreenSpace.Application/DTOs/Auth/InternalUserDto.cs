using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.Auth
{
    public class InternalUserDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// User first name
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(128, ErrorMessage = "First name cannot exceed 128 characters")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User last name
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(128, ErrorMessage = "Last name cannot exceed 128 characters")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// User phone number
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(32, ErrorMessage = "Phone number cannot exceed 32 characters")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// User address
        /// </summary>
        [MaxLength(256, ErrorMessage = "Address cannot exceed 256 characters")]
        public string? Address { get; set; }

        /// <summary>
        /// User birthday
        /// </summary>
        public DateOnly? Birthday { get; set; }

        /// <summary>
        /// Role name for the staff member (e.g., "STAFF", "MANAGER", "ADMIN")
        /// </summary>
        [Required(ErrorMessage = "Role is required")]
        [MaxLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
        public string Role { get; set; } = string.Empty;
    }
}
