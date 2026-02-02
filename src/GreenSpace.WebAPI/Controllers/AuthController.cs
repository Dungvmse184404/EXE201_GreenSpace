using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // =================================================================
        // GROUP: ĐĂNG KÝ (REGISTRATION FLOW)
        // =================================================================

        /// <summary>
        ///  Khởi tạo đăng ký.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        [HttpPost("register/initiate")]
        public async Task<IActionResult> InitiateRegister([FromBody] RegisteMailDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.RegisterMailAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// (Optional): Gửi lại OTP.
        /// Dùng khi user không nhận được mail hoặc mã hết hạn.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        [HttpPost("register/resend")]
        public async Task<IActionResult> ResendRegisterOtp([FromBody] RegisteMailDto dto)
        {
            // Tận dụng DTO của Initiate vì cũng chỉ cần mỗi Email
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.RegisterMailAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        /// <summary>
        /// Xác thực OTP.
        /// User nhập mã 6 số -&gt; Server kiểm tra -&gt; Lưu trạng thái "Đã verify" vào Cache.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        [HttpPost("register/verify")]
        public async Task<IActionResult> VerifyRegisterOtp([FromBody] VerifyRegisterOtpDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.VerifyRegisterOtpAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Hoàn tất đăng ký (Finalize).
        /// Nhập Password, Tên, SĐT -&gt; Server check Cache xem đã verify mail chưa -&gt; Tạo User.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        [HttpPost("register/finalize")]
        public async Task<IActionResult> FinalizeRegister([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Hàm RegisterAsync trong Service giờ đã có logic check Cache
            var result = await _authService.RegisterAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #region  ĐĂNG NHẬP & TOKEN (AUTHENTICATION)

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.LoginAsync(dto);
            return result.IsSuccess ? Ok(result) : Unauthorized(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken, request.AccessToken);
            return result.IsSuccess ? Ok(result) : Unauthorized(result);
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            var result = await _authService.RevokeTokenAsync(request.RefreshToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region  QUẢN TRỊ (ADMIN)

        [HttpPost("internal-user")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateInternalUser([FromBody] InternalUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.CreateInternalUserAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion



    }
    public record RefreshTokenRequest(string RefreshToken, string AccessToken);
    public record RevokeTokenRequest(string RefreshToken);
}