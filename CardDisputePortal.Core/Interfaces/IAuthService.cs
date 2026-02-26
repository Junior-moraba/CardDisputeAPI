using CardDisputePortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CardDisputePortal.Core.Interfaces
{
    public interface IAuthService
    {
        Task<string> SendOtpAsync(string phoneNumber);
        Task<AuthResponse> VerifyOtpAsync(string phoneNumber, string otp);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
    }
}
