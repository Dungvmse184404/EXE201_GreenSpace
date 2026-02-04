using GreenSpace.Application.DTOs.Payment;
using GreenSpace.Application.DTOs.PayOS;
using GreenSpace.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Interfaces.External
{
    public interface IPayOSService
    {
        /// <summary>
        /// Tạo link thanh toán PayOS
        /// </summary>
        Task<IServiceResult<PayOSResponseDto>> CreatePaymentLinkAsync(PayOSRequestDto request);

        /// <summary>
        /// Xử lý webhook từ PayOS
        /// </summary>
        //Task<IServiceResult<ProcessPaymentResultDto>> ProcessWebhookAsync(WebhookType webhookBody);
    }
}
