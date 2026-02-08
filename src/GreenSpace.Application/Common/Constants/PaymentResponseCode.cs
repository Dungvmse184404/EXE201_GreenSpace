
namespace GreenSpace.Application.Common.Constants
{
    /// <summary>
    /// Payment gateway response codes and messages
    /// </summary>
    public static class PaymentResponseCode
    {
        // ============================================
        // Common Response Codes
        // ============================================
        public const string Success = "00";
        public const string Failed = "99";

        // ============================================
        // VNPay Response Codes
        // ============================================
        public static class VNPay
        {
            public const string Success = "00";
            public const string SuspectedFraud = "07";
            public const string NotRegisteredInternetBanking = "09";
            public const string AuthenticationFailedTooManyTimes = "10";
            public const string PaymentTimeout = "11";
            public const string AccountLocked = "12";
            public const string WrongOtp = "13";
            public const string CustomerCancelled = "24";
            public const string InsufficientBalance = "51";
            public const string ExceededDailyLimit = "65";
            public const string BankMaintenance = "75";
            public const string WrongPasswordTooManyTimes = "79";

            private static readonly Dictionary<string, string> Messages = new()
            {
                { "00", "Giao dịch thành công" },
                { "07", "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)." },
                { "09", "Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng." },
                { "10", "Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần" },
                { "11", "Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch." },
                { "12", "Thẻ/Tài khoản của khách hàng bị khóa." },
                { "13", "Quý khách nhập sai mật khẩu xác thực giao dịch (OTP). Xin quý khách vui lòng thực hiện lại giao dịch." },
                { "24", "Khách hàng hủy giao dịch" },
                { "51", "Tài khoản của quý khách không đủ số dư để thực hiện giao dịch." },
                { "65", "Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày." },
                { "75", "Ngân hàng thanh toán đang bảo trì." },
                { "79", "KH nhập sai mật khẩu thanh toán quá số lần quy định. Xin quý khách vui lòng thực hiện lại giao dịch" }
            };

            /// <summary>
            /// Get VNPay response message by code
            /// </summary>
            public static string GetMessage(string responseCode)
            {
                return Messages.TryGetValue(responseCode, out var message)
                    ? message
                    : "Giao dịch thất bại";
            }

            /// <summary>
            /// Check if response code indicates success
            /// </summary>
            public static bool IsSuccess(string responseCode) => responseCode == Success;
        }

        // ============================================
        // PayOS Response Codes
        // ============================================
        public static class PayOS
        {
            public const string Success = "00";

            // Callback status values
            public const string StatusPaid = "PAID";
            public const string StatusCancelled = "CANCELLED";
            public const string StatusPending = "PENDING";
            public const string StatusProcessing = "PROCESSING";
            public const string StatusExpired = "EXPIRED";

            private static readonly Dictionary<string, string> Messages = new()
            {
                { "00", "Giao dịch thành công" },
                { "PAID", "Thanh toán thành công" },
                { "CANCELLED", "Giao dịch đã bị hủy" },
                { "PENDING", "Đang chờ thanh toán" },
                { "PROCESSING", "Đang xử lý thanh toán" },
                { "EXPIRED", "Giao dịch đã hết hạn" }
            };

            /// <summary>
            /// Get PayOS response message by code or status
            /// </summary>
            public static string GetMessage(string codeOrStatus)
            {
                return Messages.TryGetValue(codeOrStatus?.ToUpper() ?? "", out var message)
                    ? message
                    : "Giao dịch thất bại";
            }

            /// <summary>
            /// Check if status indicates success
            /// </summary>
            public static bool IsSuccess(string status) =>
                status?.ToUpper() == StatusPaid || status == Success;

            /// <summary>
            /// Check if status indicates cancellation or expiry
            /// </summary>
            public static bool IsCancelled(string status) =>
                status?.ToUpper() == StatusCancelled || status?.ToUpper() == StatusExpired;
        }
    }

    /// <summary>
    /// IPN (Instant Payment Notification) response helper
    /// </summary>
    public static class IpnResponse
    {
        public static object Success(string message = "Confirm Success") => new
        {
            RspCode = PaymentResponseCode.Success,
            Message = message
        };

        public static object Failed(string message = "Confirm Failed") => new
        {
            RspCode = PaymentResponseCode.Failed,
            Message = message
        };

        public static object FromResult(bool isSuccess, string message) => new
        {
            RspCode = isSuccess ? PaymentResponseCode.Success : PaymentResponseCode.Failed,
            Message = message
        };
    }
}
