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
    }
}