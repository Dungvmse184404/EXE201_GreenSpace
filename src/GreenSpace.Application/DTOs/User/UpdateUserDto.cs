using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.User
{
    public class UpdateUserDto
    {
        [EmailAddress]
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EditAddress { get; set; }
        public string? AdditionalAddress { get; set; }
        public string? Status { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
