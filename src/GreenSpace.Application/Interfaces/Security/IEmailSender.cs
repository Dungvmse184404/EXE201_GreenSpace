using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Security
{
    public interface IEmailSender
    {
        /// <summary>
        /// Sends the email asynchronous.
        /// </summary>
        /// <param name="emailDto">The email dto.</param>
        /// <returns></returns>
        Task SendEmailAsync(SendEmailDto emailDto);
    }
}
