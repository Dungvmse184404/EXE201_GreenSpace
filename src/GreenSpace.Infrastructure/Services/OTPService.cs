using GreenSpace.Application.Common.Mail;
using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Application.Enums;
using GreenSpace.Application.Interfaces.Security; 
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace GreenSpace.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly IDistributedCache _cache;
        private readonly IEmailSender _emailService;
        private readonly ILogger<OtpService> _logger;
        private readonly IEmailTemplateHelper _mailTemplate;

        private const int ExpiryMinutes = 5;

        public OtpService(IDistributedCache cache, IEmailSender emailService, ILogger<OtpService> logger, IEmailTemplateHelper mailTemplate)
        {
            _cache = cache;
            _emailService = emailService;
            _logger = logger;
            _mailTemplate = mailTemplate;
        }

        /// <summary>
        /// Tạo OTP, lưu vào Cache và gửi Email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<string> SendOtpAsync(string email, string subject, string type = "Verification")
        {
            var otp = GenerateSecureOtp();

            string body = string.Empty;
            int expiry = 20;

            if (type == "Register")
            {
                body = await _mailTemplate.GetAccountActivationContentAsync(email, otp, expiry);
            }
            else if (type == "ResetPassword")
            {
                body = await _mailTemplate.GetResetPasswordContentAsync(email, otp, expiry);
            }
            else
            {
                // Mặc định cho các trường hợp khác
                body = $"Mã OTP của bạn là: {otp}";
            }

            // Gửi mail
            var emailDto = new SendEmailDto
            {
                To = email,
                Subject = subject,
                Body = body
            };

            await _emailService.SendEmailAsync(emailDto);

            return otp;
        }

        /// <summary>
        /// Kiểm tra OTP có đúng không
        /// </summary>
        /// <param name="email"></param>
        /// <param name="otp"></param>
        /// <returns></returns>
        //public async Task<bool> VerifyOtpAsync(string email, string otp)
        //{
        //    var keyVerify = $"OTP:Verification:{email}";
        //    var keyReset = $"OTP:ResetPassword:{email}";

        //    var storedOtp = await _cache.GetStringAsync(keyVerify) ?? await _cache.GetStringAsync(keyReset);

        //    if (string.IsNullOrEmpty(storedOtp))
        //    {
        //        return false; // OTP không tồn tại hoặc đã hết hạn
        //    }

        //    if (storedOtp == otp)
        //    {
        //        // Verify thành công -> Xóa OTP ngay để tránh dùng lại
        //        await _cache.RemoveAsync(keyVerify);
        //        await _cache.RemoveAsync(keyReset);
        //        return true;
        //    }

        //    return false;
        //}

        public async Task<OtpResult> VerifyOtpAsync(string email, string otp, string type = "Register")
        {
            var key = $"OTP:{type}:{email}";

            var storedOtp = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(storedOtp))
            {
                return OtpResult.Expired;
            }

            if (storedOtp != otp)
            {
                return OtpResult.Invalid;
            }

            await _cache.RemoveAsync(key);
            return OtpResult.Success;
        }



        /// <summary>
        /// Generates the secure otp.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        private string GenerateSecureOtp(int length = 6)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToUInt32(bytes, 0);
            var otp = value % (int)Math.Pow(10, length);
            return otp.ToString($"D{length}");
        }
    }
}