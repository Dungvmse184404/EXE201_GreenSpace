
namespace GreenSpace.Application.DTOs.VNPay
{
    public class VNPayResponseDto
    {
        public bool Success { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? TransactionId { get; set; }
    }
}
