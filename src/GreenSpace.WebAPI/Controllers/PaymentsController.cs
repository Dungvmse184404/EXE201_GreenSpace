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

namespace GreenSpace.WebAPI.Controllers
{
    /// <summary>
    /// Payment processing endpoints (VNPay, PayOS)
    /// </summary>
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

        // ============================================
        // VNPAY ENDPOINTS
        // ============================================

            /// <summary>
            /// Create VNPay payment URL
            /// </summary>
            /// <param name="request">Payment request data</param>
            /// <returns>VNPay payment URL to redirect user</returns>
            /// <remarks>
            /// Sample request:
            ///
            ///     POST /api/payments/vnpay/create
            ///     {
            ///         "orderId": "guid",              // ID don hang can thanh toan
            ///         "amount": 270000,               // So tien (VND, toi thieu 1000)
            ///         "orderDescription": "Thanh toan don hang",  // Mo ta (optional)
            ///         "bankCode": "NCB"               // Ma ngan hang (optional, de trong = chon tai VNPay)
            ///     }
            /// </remarks>
            /// <response code="200">Payment URL generated successfully</response>
            /// <response code="400">Invalid data or order not found</response>
            /// <response code="401">Unauthorized</response>
        [HttpPost("vnpay/create")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateVNPayPayment([FromBody] VNPayRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ipAddress = VNPayLibrary.GetIpAddress(HttpContext);
            var result = await _vnPayService.CreatePaymentUrlAsync(request, ipAddress);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// VNPay callback (Return URL) - Handles redirect from VNPay after payment
        /// </summary>
        /// <param name="callback">VNPay callback parameters</param>
        /// <returns>Redirect to frontend success/failure page</returns>
        /// <response code="302">Redirect to frontend</response>
        [HttpGet("vnpay/callback")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public async Task<IActionResult> VNPayCallback([FromQuery] VNPayCallbackDto callback)
        {
            _logger.LogInformation("VNPay callback received for TxnRef: {TxnRef}", callback.vnp_TxnRef);

            var result = await _vnPayService.ProcessCallbackAsync(callback);

            if (result.IsSuccess && result.Data != null)
            {
                var frontendUrl = $"{_vnpaySettings.ReturnUrl}?orderId={result.Data.OrderId}&transactionCode={result.Data.TransactionCode}";
                return Redirect(frontendUrl);
            }

            var failureUrl = $"{_clientSettings.BackupUrl}={result.Message}";
            return Redirect(failureUrl);
        }

        /// <summary>
        /// VNPay IPN (Instant Payment Notification) - Server-to-server notification
        /// </summary>
        /// <param name="callback">VNPay IPN parameters</param>
        /// <returns>IPN response</returns>
        /// <response code="200">IPN processed</response>
        [HttpPost("vnpay/ipn")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        /// <param name="request">Payment request data</param>
        /// <returns>PayOS payment URL</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/payments/payos/create
        ///     {
        ///         "orderId": "guid",  // ID don hang can thanh toan
        ///     }
        /// </remarks>
        /// <response code="200">Payment link created successfully</response>
        /// <response code="400">Invalid data or order not found</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("payos/create")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreatePayOSPayment([FromBody] PayOSRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _payOSService.CreatePaymentLinkAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// PayOS callback (Return URL) - Handles redirect from PayOS after payment
        /// </summary>
        /// <param name="callbackDto">PayOS callback parameters</param>
        /// <returns>Redirect to frontend success/failure page</returns>
        /// <response code="302">Redirect to frontend</response>
        [HttpGet("payos/callback")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public async Task<IActionResult> PayOSCallback([FromQuery] PayOSCallBackDto callbackDto)
        {
            _logger.LogInformation(
                "PayOS callback: code={Code}, orderCode={OrderCode}, status={Status}",
                callbackDto.code, callbackDto.orderCode, callbackDto.status);

            if (callbackDto.status == PaymentResponseCode.Success)
            {
                var successUrl = $"{_clientSettings.BaseUrl}?orderCode={callbackDto.orderCode}&status=success";
                return Redirect(successUrl);
            }

            var failUrl = $"{_clientSettings.BackupUrl}?orderCode={callbackDto.orderCode}&status=failed&message={callbackDto.status}";
            return Redirect(failUrl);
        }

        /// <summary>
        /// PayOS webhook (IPN) - Server-to-server notification
        /// </summary>
        /// <param name="webhookBody">PayOS webhook data</param>
        /// <returns>Acknowledgement response</returns>
        /// <response code="200">Webhook processed</response>
        [HttpPost("payos/webhook")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PayOSWebhook([FromBody] Webhook webhookBody)
        {
            _logger.LogInformation("PayOS webhook received");

            var result = await _payOSService.ProcessWebhookAsync(webhookBody);

            return Ok(IpnResponse.FromResult(result.IsSuccess, result.Message ?? PaymentStatus.Success));
        }

        // ============================================
        // PAYMENT QUERIES
        // ============================================

        /// <summary>
        /// Get payment by ID
        /// </summary>
        /// <param name="id">Payment ID (GUID)</param>
        /// <returns>Payment data</returns>
        /// <response code="200">Payment found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Payment not found</response>
        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _paymentService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get all payments for an order
        /// </summary>
        /// <param name="orderId">Order ID (GUID)</param>
        /// <returns>List of payments</returns>
        /// <response code="200">List of payments</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("order/{orderId:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByOrderId(Guid orderId)
        {
            var result = await _paymentService.GetByOrderIdAsync(orderId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
