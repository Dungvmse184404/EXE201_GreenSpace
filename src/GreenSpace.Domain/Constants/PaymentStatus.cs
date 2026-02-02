using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Domain.Constants
{
    public static class PaymentStatus
    {
        public const string Pending = "Pending";           // Just created, waiting for user
        public const string Processing = "Processing";     // User is paying
        public const string Success = "Success";           // Payment successful
        public const string Failed = "Failed";             // Payment failed
        public const string Cancelled = "Cancelled";       // User cancelled
        public const string Expired = "Expired";           // Payment link expired
        public const string Refunded = "Refunded";         // Payment refunded
    }

    public static class PaymentGateway
    {
        public const string VNPay = "VNPay";
        public const string Momo = "Momo";
        public const string ZaloPay = "ZaloPay";
        public const string BankTransfer = "BankTransfer";
        public const string COD = "COD";
    }

}
