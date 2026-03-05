using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Services
{
    /// <summary>
    /// Interface for order notifications (email, SMS, etc.)
    /// </summary>
    public interface IOrderNotificationService
    {
        /// <summary>
        /// Send order status change notification to customer
        /// </summary>
        /// <param name="recipientEmail">Customer email address</param>
        /// <param name="recipientName">Customer display name</param>
        /// <param name="newStatus">New order status</param>
        /// <param name="oldStatus">Previous order status</param>
        /// <returns>Result of notification sending</returns>
        Task<IServiceResult<bool>> SendOrderStatusNotificationAsync(string recipientEmail, string recipientName, string newStatus, string oldStatus);
    }
}
