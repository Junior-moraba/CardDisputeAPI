using System;
using System.Collections.Generic;
using System.Text;

namespace CardDisputePortal.Core.DTOs
{
    public record SendOtpRequest(string PhoneNumber);
    public record VerifyOtpRequest(string PhoneNumber, string Otp);
    public record AuthResponse(string AccessToken, string RefreshToken, UserDto User);
    public record UserDto(Guid Id, string PhoneNumber, string Name);
    public record RefreshTokenRequest(string RefreshToken);
    public record LogoutRequest(string RefreshToken);
}
