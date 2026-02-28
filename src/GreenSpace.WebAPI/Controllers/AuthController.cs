using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    /// <summary>
    /// Authentication and Authorization endpoints
    /// </summary>
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
        // GROUP: REGISTRATION FLOW
        // =================================================================

        /// <summary>
        /// Step 1: Initiate registration - Send OTP to email
        /// </summary>
        /// <param name="dto">Email address to register</param>
        /// <returns>Success if OTP sent</returns>
        /// <response code="200">OTP sent successfully</response>
        /// <response code="400">Invalid email format or email already registered</response>
        [HttpPost("register/initiate")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InitiateRegister([FromBody] RegisteMailDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.RegisterMailAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Resend registration OTP (when user didn't receive or OTP expired)
        /// </summary>
        /// <param name="dto">Email address</param>
        /// <returns>Success if OTP resent</returns>
        /// <response code="200">OTP resent successfully</response>
        /// <response code="400">Invalid email or too many requests</response>
        [HttpPost("register/resend")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendRegisterOtp([FromBody] RegisteMailDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.RegisterMailAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Step 2: Verify registration OTP (6-digit code)
        /// </summary>
        /// <param name="dto">Email and OTP code</param>
        /// <returns>Success if OTP is valid</returns>
        /// <response code="200">OTP verified successfully</response>
        /// <response code="400">Invalid or expired OTP</response>
        [HttpPost("register/verify")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyRegisterOtp([FromBody] VerifyRegisterOtpDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.VerifyRegisterOtpAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Step 3: Complete registration (after OTP verified)
        /// </summary>
        /// <param name="dto">User details: email, password, name, phone</param>
        /// <returns>Success if user created</returns>
        /// <response code="200">Registration completed successfully</response>
        /// <response code="400">Email not verified or invalid data</response>
        [HttpPost("register/finalize")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FinalizeRegister([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // =================================================================
        // GROUP: LOGIN & TOKEN
        // =================================================================

        /// <summary>
        /// Authenticate user with email and password
        /// </summary>
        /// <param name="dto">Login credentials (email and password)</param>
        /// <returns>JWT tokens and user info</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid request format</response>
        /// <response code="401">Invalid credentials</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.LoginAsync(dto);
            if (result.IsSuccess) return Ok(result);

            var statusCode = (result as ServiceResult)?.StatusCode ?? 401;
            return StatusCode(statusCode, result);
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Current refresh token and access token</param>
        /// <returns>New JWT tokens</returns>
        /// <response code="200">Tokens refreshed successfully</response>
        /// <response code="401">Invalid or expired tokens</response>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken, request.AccessToken);
            return result.IsSuccess ? Ok(result) : Unauthorized(result);
        }

        /// <summary>
        /// Revoke refresh token (logout)
        /// </summary>
        /// <param name="request">Refresh token to revoke</param>
        /// <returns>Success if token revoked</returns>
        /// <response code="200">Token revoked successfully</response>
        /// <response code="400">Invalid token</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("revoke")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            var result = await _authService.RevokeTokenAsync(request.RefreshToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // =================================================================
        // GROUP: ADMIN
        // =================================================================

        /// <summary>
        /// Create internal user (Staff, Manager, Admin) - Admin only
        /// </summary>
        /// <param name="dto">User details with role</param>
        /// <returns>Created user info</returns>
        /// <response code="200">User created successfully</response>
        /// <response code="400">Invalid data or email already exists</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpPost("internal-user")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateInternalUser([FromBody] InternalUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.CreateInternalUserAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // =================================================================
        // GROUP: FORGOT PASSWORD FLOW
        // =================================================================

        /// <summary>
        /// Step 1: Initiate password reset - Send OTP to registered email
        /// </summary>
        /// <param name="dto">Registered email address</param>
        /// <returns>Success if OTP sent</returns>
        /// <response code="200">OTP sent successfully</response>
        /// <response code="400">Email not found or invalid format</response>
        [HttpPost("password/forgot")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ForgotPasswordAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Resend password reset OTP
        /// </summary>
        /// <param name="dto">Email address</param>
        /// <returns>Success if OTP resent</returns>
        /// <response code="200">OTP resent successfully</response>
        /// <response code="400">Invalid email or too many requests</response>
        [HttpPost("password/resend")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendResetPasswordOtp([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ForgotPasswordAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Step 2: Verify password reset OTP (valid for 15 minutes after verification)
        /// </summary>
        /// <param name="dto">Email and 6-digit OTP</param>
        /// <returns>Success if OTP valid</returns>
        /// <response code="200">OTP verified - can now reset password</response>
        /// <response code="400">Invalid or expired OTP</response>
        [HttpPost("password/verify")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyResetPasswordOtp([FromBody] VerifyResetPasswordOtpDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.VerifyResetPasswordOtpAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Step 3: Set new password (must verify OTP first within 15 minutes)
        /// </summary>
        /// <param name="dto">Email, new password, and confirm password</param>
        /// <returns>Success if password reset</returns>
        /// <response code="200">Password reset successfully - all sessions invalidated</response>
        /// <response code="400">OTP not verified, passwords don't match, or invalid password</response>
        [HttpPost("password/reset")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

    public record RefreshTokenRequest(string RefreshToken, string AccessToken);
    public record RevokeTokenRequest(string RefreshToken);
}
