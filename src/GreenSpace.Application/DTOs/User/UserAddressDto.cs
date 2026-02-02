using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.User
{
    public class UserAddressDto
    {
        public Guid AddressId { get; set; }
        public Guid UserId { get; set; }
        public string Address { get; set; } = string.Empty;
    }

    public class CreateUserAddressDto
    {
        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;
    }

    public class UpdateUserAddressDto
    {
        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;
    }
}
