using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace GreenSpace.Application.DTOs.Auth
{
    public class SendEmailDto
    {
        [Required]
        [EmailAddress]
        public string To { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public IFormFileCollection? Attachments { get; set; }
    }
}
