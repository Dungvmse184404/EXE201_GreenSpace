using GreenSpace.Application.Enums;
using GreenSpace.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Interfaces.Security
{
    /// <summary>
    /// 
    /// </summary>
    public interface IOtpService
    {
        /// <summary>
        /// Tạo OTP, lưu vào Cache và gửi Email
        /// </summary>
        Task<string> SendOtpAsync(string email, string subject, string type = "Verification");

        /// <summary>
        /// Kiểm tra OTP có đúng không
        /// </summary>
        Task<OtpResult> VerifyOtpAsync(string email, string otp, string type);

    }
}
