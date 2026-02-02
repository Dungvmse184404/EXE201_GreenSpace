using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.Payment
{
    public class ProcessPaymentResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
        public string? TransactionCode { get; set; }
        public decimal? Amount { get; set; }
        public string? BankCode { get; set; }
        public string? CardType { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
