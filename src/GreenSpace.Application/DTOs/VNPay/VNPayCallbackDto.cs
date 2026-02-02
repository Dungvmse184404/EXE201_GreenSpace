
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.Application.DTOs.VNPay
{
    public class VNPayCallbackDto
    {
        [FromQuery(Name = "vnp_TmnCode")]
        public string vnp_TmnCode { get; set; } = string.Empty;

        [FromQuery(Name = "vnp_Amount")]
        public string vnp_Amount { get; set; } = string.Empty;

        [FromQuery(Name = "vnp_BankCode")]
        public string vnp_BankCode { get; set; } = string.Empty;

        [FromQuery(Name = "vnp_BankTranNo")]
        public string vnp_BankTranNo { get; set; } = string.Empty;

        [FromQuery(Name = "vnp_CardType")]
        public string vnp_CardType { get; set; } = string.Empty;

        [FromQuery(Name = "vnp_OrderInfo")]
        public string vnp_OrderInfo { get; set; } = string.Empty;

        [FromQuery(Name = "vnp_PayDate")]
        public string vnp_PayDate { get; set; } = string.Empty;

        [FromQuery(Name = "vnp_ResponseCode")]
        public string vnp_ResponseCode { get; set; } = string.Empty;

        [FromQuery(Name = "vnp_TxnRef")]
        public string vnp_TxnRef { get; set; } = string.Empty;

        [FromQuery(Name = "vnp_TransactionNo")]
        public string vnp_TransactionNo { get; set; } = string.Empty;

        [FromQuery(Name = "vnp_TransactionStatus")]
        public string vnp_TransactionStatus { get; set; } = string.Empty;

        [FromQuery(Name = "vnp_SecureHash")]
        public string vnp_SecureHash { get; set; } = string.Empty;
    }
}
