using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Domain.Constants
{
    public static class OrderStatus
    {
        public const string Pending = "Pending"; // khi mới tạo đơn hàng
        public const string Confirmed = "Confirmed";// sau khi thanh toán thành công
        public const string Shipping = "Shipping"; 
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string Returned = "Returned";

        /// <summary>
        /// All valid order statuses
        /// </summary>
        public static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            Pending, Confirmed, Shipping, Completed, Cancelled, Returned
        };

        /// <summary>
        /// Check if a status value is valid
        /// </summary>
        public static bool IsValid(string status) =>
            !string.IsNullOrWhiteSpace(status) && ValidStatuses.Contains(status.Trim());

        /// <summary>
        /// Normalize status to canonical casing (e.g. "COMPLETED" → "Completed")
        /// </summary>
        public static string Normalize(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return status;
            var trimmed = status.Trim();
            foreach (var s in ValidStatuses)
            {
                if (string.Equals(s, trimmed, StringComparison.OrdinalIgnoreCase))
                    return s;
            }
            return trimmed;
        }

        /// <summary>
        /// Case-insensitive status comparison
        /// </summary>
        public static bool Equals(string? a, string? b) =>
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
