using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.Common.Mail;
using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Application.DTOs.Mail;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Security;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NETCore.MailKit.Core;

namespace GreenSpace.Infrastructure.Services
{
    /// <summary>
    /// Service for sending order notifications to customers
    /// </summary>
    public class OrderNotificationService : IOrderNotificationService
    {
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateHelper _emailTemplateHelper;
        private readonly ILogger<OrderNotificationService> _logger;

        public OrderNotificationService(
            IEmailSender emailSender,
            IEmailTemplateHelper emailTemplateHelper,
            ILogger<OrderNotificationService> logger)
        {
            _emailSender = emailSender;
            _emailTemplateHelper = emailTemplateHelper;
            _logger = logger;
        }

        /// <summary>
        /// Send order status change notification to customer
        /// </summary>
        public async Task<IServiceResult<bool>> SendOrderStatusNotificationAsync(
            string recipientEmail,
            string recipientName,
            string newStatus,
            string oldStatus)
        {
            try
            {
                if (string.IsNullOrEmpty(recipientEmail))
                {
                    _logger.LogWarning("Recipient email is empty, skipping notification");
                    return ServiceResult<bool>.Failure(
                        ApiStatusCodes.BadRequest,
                        "Recipient email is empty");
                }

                // Prepare email
                var emailContent = await _emailTemplateHelper
                    .GetOrderStatusNotificationContentAsync(
                        recipientName,
                        newStatus,
                        oldStatus);

                var emailMessage = new SendEmailDto
                {
                    To = recipientEmail,
                    Subject = "Thông báo cập nhật đơn hàng - GreenSpace",
                    Body = emailContent
                };

                // Send email
                await _emailSender.SendEmailAsync(emailMessage);

                _logger.LogInformation(
                    "Order status notification sent to {Email}. Status changed from {OldStatus} to {NewStatus}",
                    recipientEmail,
                    oldStatus,
                    newStatus);

                return ServiceResult<bool>.Success(true, "Notification sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error sending notification to {Email}. Status changed from {OldStatus} to {NewStatus}",
                    recipientEmail,
                    oldStatus,
                    newStatus);

                return ServiceResult<bool>.Failure(
                    ApiStatusCodes.InternalServerError,
                    $"Error sending notification: {ex.Message}");
            }
        }
    }
}
