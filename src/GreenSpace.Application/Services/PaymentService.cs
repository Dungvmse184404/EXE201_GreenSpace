using AutoMapper;
using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.Payment;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IServiceResult<PaymentDto>> GetByIdAsync(Guid paymentId)
        {
            try
            {
                var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                    return ServiceResult<PaymentDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Payment.NotFound);

                var dto = _mapper.Map<PaymentDto>(payment);
                return ServiceResult<PaymentDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {PaymentId}", paymentId);
                return ServiceResult<PaymentDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<List<PaymentDto>>> GetByOrderIdAsync(Guid orderId)
        {
            try
            {
                var payments = await _unitOfWork.PaymentRepository.GetAllQueryable()
                    .Where(p => p.OrderId == orderId)
                    .ToListAsync();

                var dtos = _mapper.Map<List<PaymentDto>>(payments);
                return ServiceResult<List<PaymentDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for order {OrderId}", orderId);
                return ServiceResult<List<PaymentDto>>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<PaymentDto>> CreateAsync(
            Guid orderId,
            string paymentMethod,
            decimal amount)
        {
            try
            {
                var payment = new Payment
                {
                    OrderId = orderId,
                    PaymentMethod = paymentMethod,
                    Amount = amount,
                    Status = "Pending"
                };

                await _unitOfWork.PaymentRepository.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                var dto = _mapper.Map<PaymentDto>(payment);
                return ServiceResult<PaymentDto>.Success(dto, ApiMessages.Payment.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return ServiceResult<PaymentDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<PaymentDto>> UpdateStatusAsync(Guid paymentId, string status)
        {
            try
            {
                var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                    return ServiceResult<PaymentDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Payment.NotFound);

                payment.Status = status;
                await _unitOfWork.PaymentRepository.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                var dto = _mapper.Map<PaymentDto>(payment);
                return ServiceResult<PaymentDto>.Success(dto, ApiMessages.Payment.StatusUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status");
                return ServiceResult<PaymentDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }
    }
}