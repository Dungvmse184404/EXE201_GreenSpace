using GreenSpace.Application.DTOs.Payment;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<IServiceResult<PaymentDto>> GetByIdAsync(Guid paymentId);
        Task<IServiceResult<List<PaymentDto>>> GetByOrderIdAsync(Guid orderId);
        Task<IServiceResult<PaymentDto>> CreateAsync(Guid orderId, string paymentMethod, decimal amount);
        Task<IServiceResult<PaymentDto>> UpdateStatusAsync(Guid paymentId, string status);
    }
}
