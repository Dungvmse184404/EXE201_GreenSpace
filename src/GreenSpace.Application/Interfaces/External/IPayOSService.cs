using GreenSpace.Application.DTOs.Payment;
using GreenSpace.Application.DTOs.PayOS;
using GreenSpace.Domain.Interfaces;
using PayOS.Models.Webhooks;


namespace GreenSpace.Application.Interfaces.External
{
    public interface IPayOSService
    {
        /// <summary>
        /// Tạo link thanh toán PayOS
        /// </summary>
        Task<IServiceResult<PayOSResponseDto>> CreatePaymentLinkAsync(PayOSRequestDto request);

        /// <summary>
        /// Xử lý callback từ PayOS (redirect URL)
        /// Gọi khi user được redirect về sau khi thanh toán
        /// </summary>
        Task<IServiceResult<ProcessPaymentResultDto>> ProcessCallbackAsync(string orderCode, string status);

        /// <summary>
        /// Xử lý webhook từ PayOS (server-to-server)
        /// </summary>
        Task<IServiceResult<ProcessPaymentResultDto>> ProcessWebhookAsync(Webhook webhookBody);
    }
}
