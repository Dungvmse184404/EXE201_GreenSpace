using GreenSpace.Application.DTOs.Payment;
using GreenSpace.Application.DTOs.VNPay;
using GreenSpace.Domain.Interfaces;


namespace GreenSpace.Application.Interfaces.External
{
    public interface IVNPayService
    {
        /// <summary>
        /// Create VNPay payment URL
        /// </summary>
        Task<IServiceResult<VNPayResponseDto>> CreatePaymentUrlAsync(
            VNPayRequestDto request,
            string ipAddress);

        /// <summary>
        /// Process VNPay callback (IPN)
        /// </summary>
        Task<IServiceResult<ProcessPaymentResultDto>> ProcessCallbackAsync(
            VNPayCallbackDto callback);

        /// <summary>
        /// Validate VNPay signature
        /// </summary>
        bool ValidateSignature(
            Dictionary<string, string> vnpayData,
            string secureHash);

        /// <summary>
        /// Get payment by transaction reference
        /// </summary>
        Task<IServiceResult<PaymentDto>> GetPaymentByTxnRefAsync(string txnRef);
    }
}
