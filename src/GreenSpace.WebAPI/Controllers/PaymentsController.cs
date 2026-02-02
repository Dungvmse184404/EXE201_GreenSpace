using GreenSpace.Application.Common.Settings;
using GreenSpace.Application.DTOs.Payment;
using GreenSpace.Application.DTOs.VNPay;
using GreenSpace.Application.Interfaces.External;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IOptions<ClientSettings> clientSettings,
            IOptions<VNPaySettings> settings,
            IVNPayService vnPayService,
            IPaymentService paymentService,
            ILogger<PaymentsController> logger)
        {
            _clientSettings = clientSettings.Value;
            _vnpaySettings = settings.Value;
            _vnPayService = vnPayService;
            _paymentService = paymentService;
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

            return Ok(new
            {
                RspCode = result.IsSuccess ? "00" : "99",
                Message = result.Message
            });
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