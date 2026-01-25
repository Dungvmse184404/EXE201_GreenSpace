
using GreenSpace.Application.Common.Mail;
using Microsoft.AspNetCore.Hosting;

namespace GreenSpace.Infrastructure.Helpers
{
    public class EmailTemplateHelper : IEmailTemplateHelper
    {
        private readonly IWebHostEnvironment _env;
        public EmailTemplateHelper(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// hàm gửi mail đặt lại mật khẩu
        /// </summary>
        /// <param name="recipientName">Name of the recipient.</param>
        /// <param name="otp">The otp.</param>
        /// <param name="expiry">The expiry.</param>
        /// <returns></returns>
        public async Task<string> GetResetPasswordContentAsync(string recipientName, string otp, int expiry)
        {
            var template = await ReadTemplateAsync("ResetPassword.html");

            return template
                .Replace("{RecipientName}", recipientName)
                .Replace("{OtpCode}", otp)
                .Replace("{ExpiryMinutes}", expiry.ToString());
        }

        /// <summary>
        /// hàm gửi mail kích hoạt tài khoản
        /// </summary>
        /// <param name="recipientName">Name of the recipient.</param>
        /// <param name="otp">The otp.</param>
        /// <param name="expiry">The expiry.</param>
        /// <returns></returns>
        public async Task<string> GetAccountActivationContentAsync(string recipientName, string otp, int expiry)
        {
            var template = await ReadTemplateAsync("AccountActivation.html");

            return template
                .Replace("{RecipientName}", recipientName)
                .Replace("{OtpCode}", otp)
                .Replace("{ExpiryMinutes}", expiry.ToString());
        }



        /// <summary>
        /// hàm đọc template mail từ file
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException">Không tìm thấy template mail: {templateName}</exception>
        private async Task<string> ReadTemplateAsync(string templateName)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "wwwroot", "Templates", "Emails", templateName);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Không tìm thấy template mail: {templateName}");

            return await File.ReadAllTextAsync(filePath);
        }
    }
}
