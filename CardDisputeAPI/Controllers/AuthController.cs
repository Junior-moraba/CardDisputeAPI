using CardDisputePortal.Core.DTOs;
using CardDisputePortal.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CardDisputeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            var otp = await _authService.SendOtpAsync(request.PhoneNumber);
            // Return OTP in response for development/testing (remove in production)
            return Ok(new { success = true, message = "OTP sent successfully", otp, expiresIn = 300 });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var response = await _authService.VerifyOtpAsync(request.PhoneNumber, request.Otp);
            return Ok(new { success = true, data = response });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { success = false, message = "RefreshToken is required." });

            try
            {
                var response = await _authService.RefreshTokenAsync(request.RefreshToken);
                return Ok(new { success = true, data = response });
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { success = false, message = "RefreshToken is required." });

            await _authService.LogoutAsync(request.RefreshToken);
            return Ok(new { success = true, message = "Logged out." });
        }
    }
}