using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.Common.Settings;
using GreenSpace.Application.DTOs.Payment;
using GreenSpace.Application.DTOs.VNPay;
using GreenSpace.Application.Enums;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.External;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Constants;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace GreenSpace.Infrastructure.ExternalServices
{
    public class VNPayService : IVNPayService
    {
        private readonly VNPaySettings _settings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VNPayService> _logger;

        public VNPayService(
            IOptions<VNPaySettings> settings,
            IUnitOfWork unitOfWork,
            ILogger<VNPayService> logger)
        {
            _settings = settings.Value;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IServiceResult<VNPayResponseDto>> CreatePaymentUrlAsync(
            VNPayRequestDto request,
            string ipAddress)
        {
            try
            {
                // 1. Validate order exists
                var order = await _unitOfWork.OrderRepository.GetByIdAsync(request.OrderId);
                if (order == null)
                {
                    return ServiceResult<VNPayResponseDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Order.NotFound );
                }

                // create transaction reference (unique identifier)
                var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
                var tick = timeNow.Ticks.ToString();
                var txnRef = $"VNPAY_{request.OrderId}_{tick}";

                var payment = new Payment
                {
                    OrderId = request.OrderId,
                    Gateway = PaymentGateway.VNPay,
                    PaymentMethod = request.BankCode ?? "VNPay",
                    TransactionRef = txnRef,
                    Amount = request.Amount,
                    Status = PaymentStatus.Pending,
                    BankCode = request.BankCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddMinutes(_settings.TimeoutMinutes)
                };

                await _unitOfWork.PaymentRepository.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Payment created: {PaymentId} for Order {OrderId}",
                    payment.PaymentId,
                    request.OrderId);

                // 4. Generate VNPay URL
                var vnpay = new VNPayLibrary();

                vnpay.AddRequestData("vnp_Version", _settings.Version);
                vnpay.AddRequestData("vnp_Command", _settings.Command);
                vnpay.AddRequestData("vnp_TmnCode", _settings.TmnCode);
                vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());
                vnpay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", _settings.CurrCode);
                vnpay.AddRequestData("vnp_IpAddr", ipAddress);
                vnpay.AddRequestData("vnp_Locale", _settings.Locale);
                vnpay.AddRequestData("vnp_OrderInfo",
                    request.OrderDescription ?? $"Thanh toan don hang #{order.OrderId}");
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", _settings.ReturnUrl);
                vnpay.AddRequestData("vnp_TxnRef", txnRef); // Use our transaction ref

                if (!string.IsNullOrEmpty(request.BankCode))
                {
                    vnpay.AddRequestData("vnp_BankCode", request.BankCode);
                }

                var paymentUrl = vnpay.CreateRequestUrl(_settings.BaseUrl, _settings.HashSecret);

                // 5. Update payment with URL
                payment.PaymentUrl = paymentUrl;
                payment.Status = PaymentStatus.Processing; // User is redirected to pay
                await _unitOfWork.PaymentRepository.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "VNPay URL created for Payment {PaymentId}, TxnRef: {TxnRef}",
                    payment.PaymentId,
                    txnRef);

                return ServiceResult<VNPayResponseDto>.Success(
                    new VNPayResponseDto
                    {
                        Success = true,
                        PaymentUrl = paymentUrl,
                        TransactionId = txnRef,
                        Message = "Payment URL created successfully"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VNPay payment URL");
                 return ServiceResult<VNPayResponseDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }


        public async Task<IServiceResult<ProcessPaymentResultDto>> ProcessCallbackAsync(
           VNPayCallbackDto callback)
        {
            try
            {
                // Validate signature
                var vnpayData = ConvertCallbackToDictionary(callback);
                if (!ValidateSignature(vnpayData, callback.vnp_SecureHash))
                {
                    _logger.LogWarning("Invalid VNPay signature for TxnRef: {TxnRef}", callback.vnp_TxnRef);
                      return ServiceResult<ProcessPaymentResultDto>.Failure(ApiStatusCodes.Conflict, ApiMessages.Payment.InvalidSignature);
                }

                // Find payment by transaction reference
                var payment = await _unitOfWork.PaymentRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(p => p.TransactionRef == callback.vnp_TxnRef);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for TxnRef: {TxnRef}", callback.vnp_TxnRef);
                    return ServiceResult<ProcessPaymentResultDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Payment.NotFound);
                }

                // Check if already processed (prevent double processing)
                if (payment.Status == PaymentStatus.Success)
                {
                    _logger.LogInformation("Payment {PaymentId} already processed", payment.PaymentId);
                    return ServiceResult<ProcessPaymentResultDto>.Success(
                        new ProcessPaymentResultDto
                        {
                            Success = true,
                            Message = "Payment already processed",
                            OrderId = payment.OrderId,
                            TransactionCode = payment.TransactionCode,
                            Amount = payment.Amount
                        });
                }

                // Parse payment details
                var amount = decimal.Parse(callback.vnp_Amount) / 100;
                DateTime? paymentDate = null;
                if (DateTime.TryParseExact(
                    callback.vnp_PayDate,
                    "yyyyMMddHHmmss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
                {
                    paymentDate = parsedDate;
                }

                // Check payment success
                var isSuccess = callback.vnp_ResponseCode == "00" &&
                                callback.vnp_TransactionStatus == "00";

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Update payment record
                    payment.Status = isSuccess ? PaymentStatus.Success : PaymentStatus.Failed;
                    payment.TransactionCode = callback.vnp_TransactionNo;
                    payment.ResponseCode = callback.vnp_ResponseCode;
                    payment.ResponseMessage = GetVNPayResponseMessage(callback.vnp_ResponseCode);
                    payment.BankCode = callback.vnp_BankCode;
                    payment.CardType = callback.vnp_CardType;
                    payment.PaidAt = paymentDate;
                    payment.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.PaymentRepository.UpdateAsync(payment);

                    // Update order status if payment successful
                    if (isSuccess)
                    {
                        var order = await _unitOfWork.OrderRepository.GetByIdAsync(payment.OrderId);
                        if (order != null)
                        {
                            order.Status = OrderStatus.Confirmed;
                            await _unitOfWork.OrderRepository.UpdateAsync(order);
                        }
                    }
                    var saved = await _unitOfWork.SaveChangesAsync();

                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation(
                        "Payment {PaymentId} processed: Status={Status}, Amount={Amount}",
                        payment.PaymentId,
                        payment.Status,
                        payment.Amount);

                    return ServiceResult<ProcessPaymentResultDto>.Success(
                        new ProcessPaymentResultDto
                        {
                            Success = isSuccess,
                            Message = isSuccess ? ApiMessages.Payment.Success : payment.ResponseMessage ?? ApiMessages.Payment.Failed,
                            OrderId = payment.OrderId,
                            TransactionCode = callback.vnp_TransactionNo,
                            Amount = amount,
                            BankCode = callback.vnp_BankCode,
                            CardType = callback.vnp_CardType,
                            PaymentDate = paymentDate
                        });
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Error in payment transaction");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay callback");
                return ServiceResult<ProcessPaymentResultDto>
                    .Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }


        public bool ValidateSignature(Dictionary<string, string> vnpayData, string secureHash)
        {
            var vnpay = new VNPayLibrary();

            foreach (var (key, value) in vnpayData)
            {
                if (!string.IsNullOrEmpty(value) && key != "vnp_SecureHash")
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            return vnpay.ValidateSignature(secureHash, _settings.HashSecret);
        }

        public async Task<IServiceResult<PaymentDto>> GetPaymentByTxnRefAsync(string txnRef)
        {
            try
            {
                var payment = await _unitOfWork.PaymentRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(p => p.TransactionRef == txnRef);

                if (payment == null)
                {
                    return ServiceResult<PaymentDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Payment.NotFound);
                }

                var dto = new PaymentDto
                {
                    PaymentId = payment.PaymentId,
                    OrderId = payment.OrderId,
                    Gateway = payment.Gateway,
                    PaymentMethod = payment.PaymentMethod ?? "",
                    Amount = payment.Amount,
                    TransactionCode = payment.TransactionCode,
                    Status = payment.Status ?? "",
                    PaidAt = payment.PaidAt,
                    BankCode = payment.BankCode,
                    CardType = payment.CardType
                };

                return ServiceResult<PaymentDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment by TxnRef: {TxnRef}", txnRef);
                return ServiceResult<PaymentDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        private Dictionary<string, string> ConvertCallbackToDictionary(VNPayCallbackDto callback)
        {
            return new Dictionary<string, string>
            {
                { "vnp_TmnCode", callback.vnp_TmnCode },
                { "vnp_Amount", callback.vnp_Amount },
                { "vnp_BankCode", callback.vnp_BankCode },
                { "vnp_BankTranNo", callback.vnp_BankTranNo },
                { "vnp_CardType", callback.vnp_CardType },
                { "vnp_OrderInfo", callback.vnp_OrderInfo },
                { "vnp_PayDate", callback.vnp_PayDate },
                { "vnp_ResponseCode", callback.vnp_ResponseCode },
                { "vnp_TxnRef", callback.vnp_TxnRef },
                { "vnp_TransactionNo", callback.vnp_TransactionNo },
                { "vnp_TransactionStatus", callback.vnp_TransactionStatus }
            };
        }

        private string GetVNPayResponseMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
                "09" => "Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
                "10" => "Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
                "12" => "Thẻ/Tài khoản của khách hàng bị khóa.",
                "13" => "Quý khách nhập sai mật khẩu xác thực giao dịch (OTP). Xin quý khách vui lòng thực hiện lại giao dịch.",
                "24" => "Khách hàng hủy giao dịch",
                "51" => "Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
                "65" => "Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "79" => "KH nhập sai mật khẩu thanh toán quá số lần quy định. Xin quý khách vui lòng thực hiện lại giao dịch",
                _ => "Giao dịch thất bại"
            };
        }
    }
}