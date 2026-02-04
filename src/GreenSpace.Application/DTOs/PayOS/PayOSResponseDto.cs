using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.PayOS
{
    public class PayOSResponseDto
    {
        public bool Success { get; set; }
        public string? PaymentUrl { get; set; }
        public string? QrCode { get; set; }
        public string? Message { get; set; }
        public string? TransactionId { get; set; }
    }
}
