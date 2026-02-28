using System.ComponentModel.DataAnnotations;

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
    }
}
