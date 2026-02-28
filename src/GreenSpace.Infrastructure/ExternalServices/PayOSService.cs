using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.Common.Settings;
using GreenSpace.Application.DTOs.Payment;
using GreenSpace.Application.DTOs.PayOS;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.External;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Constants;
using GreenSpace.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

using GreenSpace.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Infrastructure.ExternalServices
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOSClient _payOS;
        private readonly PayOSSettings _settings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockService _stockService;
        private readonly ILogger<PayOSService> _logger;

        public PayOSService(
            IOptions<PayOSSettings> settings,
            IUnitOfWork unitOfWork,
            IStockService stockService,
            ILogger<PayOSService> logger)
        {
            _settings = settings.Value;
            _unitOfWork = unitOfWork;
            _stockService = stockService;
            _logger = logger;

            _payOS = new PayOSClient(new PayOSOptions
            {
                ClientId = _settings.ClientId,
                ApiKey = _settings.ApiKey,
                ChecksumKey = _settings.ChecksumKey
            });
        }

        public async Task<IServiceResult<PayOSResponseDto>> CreatePaymentLinkAsync(PayOSRequestDto request)
        {
            try
            {
                // 1. Validate order exists
                var order = await _unitOfWork.OrderRepository.GetByIdAsync(request.OrderId);
                if (order == null)
                {
                    return ServiceResult<PayOSResponseDto>.Failure(
                        ApiStatusCodes.NotFound, ApiMessages.Order.NotFound);
                }

                // 2. Generate unique orderCode (PayOS requires numeric long)
                var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000000000;

                // 3. Create payment record
                var payment = new Payment
                {
                    OrderId = request.OrderId,
                    Gateway = PaymentGateway.PayOS,
                    PaymentMethod = "PayOS",
                    TransactionRef = orderCode.ToString(),
                    Amount = order.TotalAmount,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddMinutes(15)
                };

                await _unitOfWork.PaymentRepository.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                // 4. Create PayOS payment link
                var description = $"GreenSpace #{order.OrderId.ToString()[..8]}";
                // PayOS description max 25 chars
                if (description.Length > 25)
                    description = description[..25];

                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = (long)order.TotalAmount,
                    Description = description,
                    CancelUrl = _settings.CancelUrl,
                    ReturnUrl = _settings.ReturnUrl
                };

                var createPaymentResult = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

                // 5. Update payment with URL
                payment.PaymentUrl = createPaymentResult.CheckoutUrl;
                payment.Status = PaymentStatus.Processing;
                await _unitOfWork.PaymentRepository.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "PayOS payment link created: PaymentId={PaymentId}, OrderCode={OrderCode}",
                    payment.PaymentId, orderCode);

                return ServiceResult<PayOSResponseDto>.Success(
                    new PayOSResponseDto
                    {
                        Success = true,
                        PaymentUrl = createPaymentResult.CheckoutUrl,
                        QrCode = createPaymentResult.QrCode,
                        TransactionId = orderCode.ToString(),
                        Message = ApiMessages.Payment.LinkCreated
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment link");
                return ServiceResult<PayOSResponseDto>.Failure(
                    ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<ProcessPaymentResultDto>> ProcessCallbackAsync(string orderCode, string status)
        {
            try
            {
                _logger.LogInformation(
                    "Processing PayOS callback: OrderCode={OrderCode}, Status={Status}",
                    orderCode, status);

                // 1. Find payment by orderCode (stored as TransactionRef)
                var payment = await _unitOfWork.PaymentRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(p => p.TransactionRef == orderCode
                                           && p.Gateway == PaymentGateway.PayOS);

                if (payment == null)
                {
                    _logger.LogWarning("PayOS payment not found for OrderCode: {OrderCode}", orderCode);
                    return ServiceResult<ProcessPaymentResultDto>.Failure(
                        ApiStatusCodes.NotFound, ApiMessages.Payment.NotFound);
                }

                // 2. Prevent double processing
                if (payment.Status == PaymentStatus.Success)
                {
                    _logger.LogInformation("PayOS payment {PaymentId} already processed as success", payment.PaymentId);
                    return ServiceResult<ProcessPaymentResultDto>.Success(
                        new ProcessPaymentResultDto
                        {
                            Success = true,
                            Message = ApiMessages.Payment.Paied,
                            OrderId = payment.OrderId,
                            Amount = payment.Amount
                        });
                }

                if (payment.Status == PaymentStatus.Failed)
                {
                    _logger.LogInformation("PayOS payment {PaymentId} already processed as failed", payment.PaymentId);
                    return ServiceResult<ProcessPaymentResultDto>.Success(
                        new ProcessPaymentResultDto
                        {
                            Success = false,
                            Message = ApiMessages.Payment.Failed,
                            OrderId = payment.OrderId,
                            Amount = payment.Amount
                        });
                }

                // 3. Determine success based on callback status using constants
                var isSuccess = PaymentResponseCode.PayOS.IsSuccess(status);
                var isCancelled = PaymentResponseCode.PayOS.IsCancelled(status);

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 4. Update payment status
                    if (isSuccess)
                    {
                        payment.Status = PaymentStatus.Success;
                        payment.PaidAt = DateTime.UtcNow;
                    }
                    else if (isCancelled)
                    {
                        payment.Status = PaymentStatus.Failed;
                        payment.ResponseMessage = PaymentResponseCode.PayOS.GetMessage(status);
                    }
                    // If PENDING or PROCESSING, don't change status yet

                    payment.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.PaymentRepository.UpdateAsync(payment);

                    // 5. Update order and stock
                    var order = await _unitOfWork.OrderRepository.GetByIdAsync(payment.OrderId);
                    if (order != null)
                    {
                        if (isSuccess)
                        {
                            order.Status = OrderStatus.Confirmed;
                            await _unitOfWork.OrderRepository.UpdateAsync(order);
                            await _stockService.ConfirmStockReservationAsync(order.OrderId);
                            _logger.LogInformation("Order {OrderId} confirmed, stock reservation confirmed", order.OrderId);
                        }
                        else if (isCancelled)
                        {
                            order.Status = OrderStatus.Cancelled;
                            await _unitOfWork.OrderRepository.UpdateAsync(order);
                            await _stockService.RevertStockReservationAsync(order.OrderId);
                            _logger.LogInformation("Order {OrderId} cancelled, stock reverted", order.OrderId);
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation(
                        "PayOS callback processed: PaymentId={PaymentId}, Status={Status}",
                        payment.PaymentId, payment.Status);

                    return ServiceResult<ProcessPaymentResultDto>.Success(
                        new ProcessPaymentResultDto
                        {
                            Success = isSuccess,
                            Message = isSuccess ? ApiMessages.Payment.Success : ApiMessages.Payment.Failed,
                            OrderId = payment.OrderId,
                            Amount = payment.Amount
                        });
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Error in PayOS callback transaction");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS callback");
                return ServiceResult<ProcessPaymentResultDto>.Failure(
                    ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<ProcessPaymentResultDto>> ProcessWebhookAsync(Webhook webhookBody)
        {
            try
            {
                // 1. Verify webhook signature
                WebhookData verifiedData = await _payOS.Webhooks.VerifyAsync(webhookBody);

                var orderCode = verifiedData.OrderCode.ToString();

                // 2. Find payment by orderCode (stored as TransactionRef)
                var payment = await _unitOfWork.PaymentRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(p => p.TransactionRef == orderCode
                                           && p.Gateway == PaymentGateway.PayOS);

                if (payment == null)
                {
                    _logger.LogWarning("PayOS payment not found for OrderCode: {OrderCode}", orderCode);
                    return ServiceResult<ProcessPaymentResultDto>.Failure(
                        ApiStatusCodes.NotFound, ApiMessages.Payment.NotFound);
                }

                // 3. Prevent double processing
                if (payment.Status == PaymentStatus.Success)
                {
                    _logger.LogInformation("PayOS payment {PaymentId} already processed", payment.PaymentId);
                    return ServiceResult<ProcessPaymentResultDto>.Success(
                        new ProcessPaymentResultDto
                        {
                            Success = true,
                            Message = ApiMessages.Payment.Paied,
                            OrderId = payment.OrderId,
                            Amount = payment.Amount
                        });
                }

                // 4. Determine success based on webhook code using constants
                var isSuccess = verifiedData.Code == PaymentResponseCode.PayOS.Success;

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 5. Update payment
                    payment.Status = isSuccess ? PaymentStatus.Success : PaymentStatus.Failed;
                    payment.TransactionCode = verifiedData.Reference;
                    payment.ResponseCode = verifiedData.Code;
                    payment.ResponseMessage = verifiedData.Description;
                    payment.PaidAt = isSuccess ? DateTime.UtcNow : null;
                    payment.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.PaymentRepository.UpdateAsync(payment);

                    // 6. Update order status
                    var order = await _unitOfWork.OrderRepository.GetByIdAsync(payment.OrderId);
                    if (order != null)
                    {
                        if (isSuccess)
                        {
                            order.Status = OrderStatus.Confirmed;
                            await _unitOfWork.OrderRepository.UpdateAsync(order);
                            await _stockService.ConfirmStockReservationAsync(order.OrderId);
                        }
                        else
                        {
                            order.Status = OrderStatus.Cancelled;  // Sửa từ Pending thành Cancelled
                            await _unitOfWork.OrderRepository.UpdateAsync(order);
                            await _stockService.RevertStockReservationAsync(order.OrderId);
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation(
                        "PayOS payment {PaymentId} processed: Status={Status}, Amount={Amount}",
                        payment.PaymentId, payment.Status, payment.Amount);

                    return ServiceResult<ProcessPaymentResultDto>.Success(
                        new ProcessPaymentResultDto
                        {
                            Success = isSuccess,
                            Message = isSuccess ? ApiMessages.Payment.Success : ApiMessages.Payment.Failed,
                            OrderId = payment.OrderId,
                            TransactionCode = verifiedData.Reference,
                            Amount = verifiedData.Amount
                        });
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Error in PayOS payment transaction");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS webhook");
                return ServiceResult<ProcessPaymentResultDto>.Failure(
                    ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }
    }
}
