using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.Common.Settings;
using GreenSpace.Application.DTOs.Payment;
using GreenSpace.Application.DTOs.PayOS;
using GreenSpace.Application.DTOs.VNPay;
using GreenSpace.Application.Interfaces.External;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Constants;
using GreenSpace.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PayOS.Models.Webhooks;
using System.Runtime;

namespace GreenSpace.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly VNPaySettings _vnpaySettings;
        private readonly ClientSettings _clientSettings;
        private readonly IVNPayService _vnPayService;
        private readonly IPayOSService _payOSService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IOptions<ClientSettings> clientSettings,
            IOptions<VNPaySettings> settings,
            IVNPayService vnPayService,
            IPaymentService paymentService,
            IPayOSService payOSService,
            ILogger<PaymentsController> logger)
        {
            _clientSettings = clientSettings.Value;
            _vnpaySettings = settings.Value;
            _vnPayService = vnPayService;
            _paymentService = paymentService;
            _payOSService = payOSService;
            _logger = logger;
        }
        
        /// <summary>
        /// Create VNPay payment URL
        /// </summary>
        [HttpPost("vnpay/create")]
        [Authorize]
        public async Task<IActionResult> CreateVNPayPayment([FromBody] VNPayRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ipAddress = VNPayLibrary.GetIpAddress(HttpContext);
            //request.IpAddress = ipAddress;
            var result = await _vnPayService.CreatePaymentUrlAsync(request, ipAddress);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// VNPay callback (Return URL)
        /// </summary>
        [HttpGet("vnpay/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VNPayCallback([FromQuery] VNPayCallbackDto callback)
        {
            _logger.LogInformation("VNPay callback received for TxnRef: {TxnRef}", callback.vnp_TxnRef);

            var result = await _vnPayService.ProcessCallbackAsync(callback);

            if (result.IsSuccess && result.Data != null)
            {
                // Redirect to success page on your frontend
                var frontendUrl = $"{_vnpaySettings.ReturnUrl}?orderId={result.Data.OrderId}&transactionCode={result.Data.TransactionCode}"; ;
                return Redirect(frontendUrl);
            }

            // Redirect to failure page
            var failureUrl = $"{_clientSettings.BackupUrl}={result.Message}";
            return Redirect(failureUrl);
        }

        /// <summary>
        /// VNPay IPN (Instant Payment Notification)
        /// </summary>
        [HttpPost("vnpay/ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VNPayIPN([FromQuery] VNPayCallbackDto callback)
        {
            _logger.LogInformation("VNPay IPN received for TxnRef: {TxnRef}", callback.vnp_TxnRef);

            var result = await _vnPayService.ProcessCallbackAsync(callback);

            return Ok(IpnResponse.FromResult(result.IsSuccess, result.Message ?? PaymentStatus.Success));
        }

        // ============================================
        // PAYOS ENDPOINTS
        // ============================================

        /// <summary>
        /// Create PayOS payment link
        /// </summary>
        [HttpPost("payos/create")]
        [Authorize]
        public async Task<IActionResult> CreatePayOSPayment([FromBody] PayOSRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _payOSService.CreatePaymentLinkAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// PayOS callback (Return URL - redirect sau khi thanh toán)
        /// Xử lý payment và order trước khi redirect về frontend
        /// </summary>
        [HttpGet("payos/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSCallback(
            [FromQuery] PayOSCallBackDto callbackDto)
        {
            _logger.LogInformation(
                "PayOS callback: code={Code}, orderCode={OrderCode}, status={Status}",
                callbackDto.code, callbackDto.orderCode, callbackDto.status);

            // Xử lý payment và order
            //var result = await _payOSService.ProcessCallbackAsync(
            //    callbackDto.orderCode.ToString(),
            //    callbackDto.status ?? callbackDto.code ?? "");

            if (callbackDto.status == PaymentResponseCode.Success)
            {
                var successUrl = $"{_clientSettings.BaseUrl}?orderCode={callbackDto.orderCode}&status=success";
                return Redirect(successUrl);
            }

            var failUrl = $"{_clientSettings.BackupUrl}?orderCode={callbackDto.orderCode}&status=failed&message={callbackDto.status}";
            return Redirect(failUrl);
        }

        /// <summary>
        /// PayOS webhook (IPN - server-to-server notification)
        /// </summary>
        [HttpPost("payos/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSWebhook([FromBody] Webhook webhookBody)
        {
            _logger.LogInformation("PayOS webhook received");

            var result = await _payOSService.ProcessWebhookAsync(webhookBody);

            // PayOS expects HTTP 200 to acknowledge
            return Ok(IpnResponse.FromResult(result.IsSuccess, result.Message ?? PaymentStatus.Success));
        }


        /// <summary>
        /// Get payment by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _paymentService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get payments by order ID
        /// </summary>
        [HttpGet("order/{orderId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetByOrderId(Guid orderId)
        {
            var result = await _paymentService.GetByOrderIdAsync(orderId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}