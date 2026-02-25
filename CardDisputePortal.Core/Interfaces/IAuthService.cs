using CardDisputePortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardDisputePortal.Core.Interfaces
{
    public interface IAuthService
    {
        Task<string> SendOtpAsync(string phoneNumber);
        Task<AuthResponse> VerifyOtpAsync(string phoneNumber, string otp);
    }
}
