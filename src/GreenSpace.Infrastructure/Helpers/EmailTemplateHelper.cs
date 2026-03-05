using GreenSpace.Application.Common.Mail;
using GreenSpace.Application.Common.Settings;
using GreenSpace.Domain.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

namespace GreenSpace.Infrastructure.Helpers
{
    public class EmailTemplateHelper : IEmailTemplateHelper
    {
        private readonly IWebHostEnvironment _env;
        private readonly ClientSettings _clientSettings;
        public EmailTemplateHelper(IWebHostEnvironment env, IOptions<ClientSettings> clientSettings)
        {
            _env = env;
            _clientSettings = clientSettings.Value;
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
        /// hàm gửi mail thông báo thay đổi trạng thái đơn hàng
        /// </summary>
        /// <param name="recipientName">Name of the recipient.</param>
        /// <param name="orderId">Order ID</param>
        /// <param name="newStatus">New order status</param>
        /// <param name="oldStatus">Previous order status</param>
        /// <returns></returns>
        public async Task<string> GetOrderStatusNotificationContentAsync(string recipientName, string newStatus, string oldStatus)
        {
            var template = await ReadTemplateAsync("OrderStatusNotification.html");
            string tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
             ? "SE Asia Standard Time"
             : "Asia/Ho_Chi_Minh";
            DateTime vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(tzId));
            return template
                .Replace("{RecipientName}", recipientName)
                .Replace("{OldStatus}", GetStatusDisplay(oldStatus))
                .Replace("{NewStatus}", GetStatusDisplay(newStatus))
                .Replace("{OrderPageUrl}", _clientSettings.OrderPageUrl ?? "#")
                .Replace("{CurrentDateTime}", vnTime.ToString("dd/MM/yyyy HH:mm:ss"));
        }

        /// <summary>
        /// Get display name for order status
        /// </summary>
        private string GetStatusDisplay(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return string.Empty;

            var s = status.Trim();

            return s switch
            {
                OrderStatus.Pending   => "Chờ xác nhận",
                OrderStatus.Confirmed => "Đã xác nhận",
                OrderStatus.Shipping  => "Đang giao hàng",
                OrderStatus.Completed => "Đã giao",
                OrderStatus.Cancelled => "Đã hủy",
                OrderStatus.Returned  => "Đã trả hàng",
                _ => s
            };
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
