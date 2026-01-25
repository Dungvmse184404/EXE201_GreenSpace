using GreenSpace.Application.Common.Mail;
using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Application.Enums;
using GreenSpace.Application.Interfaces.Security;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace GreenSpace.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly IDistributedCache _cache;
        private readonly IEmailSender _emailService;
        private readonly ILogger<OtpService> _logger;
        private readonly IEmailTemplateHelper _mailTemplate;

        private const int ExpiryMinutes = 20;

        public OtpService(
            IDistributedCache cache,
            IEmailSender emailService,
            ILogger<OtpService> logger,
            IEmailTemplateHelper mailTemplate)
        {
            _cache = cache;
            _emailService = emailService;
            _logger = logger;
            _mailTemplate = mailTemplate;
        }

        /// <summary>
        /// Generate OTP, save to cache, and send email
        /// </summary>
        public async Task<string> SendOtpAsync(string email, string subject, string type = "Verification")
        {
            var otp = GenerateSecureOtp();
            int expiry = ExpiryMinutes;

            // Prepare email body based on type
            string body = type switch
            {
                "Register" => await _mailTemplate.GetAccountActivationContentAsync(email, otp, expiry),
                "ResetPassword" => await _mailTemplate.GetResetPasswordContentAsync(email, otp, expiry),
                _ => $"Mã OTP của bạn là: {otp}"
            };

            var cacheKey = $"OTP:{type}:{email}";
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expiry)
            };

            try
            {
                await _cache.SetStringAsync(cacheKey, otp, cacheOptions);
                _logger.LogInformation("OTP saved to cache for {Email} with key {CacheKey}", email, cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save OTP to cache for {Email}", email);
                throw new InvalidOperationException("Unable to save OTP. Please try again.", ex);
            }

            // Send email
            var emailDto = new SendEmailDto
            {
                To = email,
                Subject = subject,
                Body = body
            };

            try
            {
                await _emailService.SendEmailAsync(emailDto);
                _logger.LogInformation("OTP email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", email);
                await _cache.RemoveAsync(cacheKey);
                throw;
            }

            return otp;
        }

        /// <summary>
        /// Verify OTP from cache
        /// </summary>
        public async Task<OtpResult> VerifyOtpAsync(string email, string otp, string type = "Register")
        {
            var key = $"OTP:{type}:{email}";

            try
            {
                var storedOtp = await _cache.GetStringAsync(key);

                if (string.IsNullOrEmpty(storedOtp))
                {
                    _logger.LogWarning("OTP expired or not found for {Email}", email);
                    return OtpResult.Expired;
                }

                if (storedOtp != otp)
                {
                    _logger.LogWarning("Invalid OTP attempt for {Email}", email);
                    return OtpResult.Invalid;
                }

                // Valid OTP - remove from cache
                await _cache.RemoveAsync(key);
                _logger.LogInformation("OTP verified successfully for {Email}", email);
                return OtpResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for {Email}", email);
                return OtpResult.Invalid;
            }
        }

        /// <summary>
        /// Generate secure 6-digit OTP
        /// </summary>
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