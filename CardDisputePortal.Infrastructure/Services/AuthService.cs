using CardDisputePortal.Core.DTOs;
using CardDisputePortal.Core.Entities;
using CardDisputePortal.Core.Interfaces;
using CardDisputePortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace CardDisputePortal.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private static readonly ConcurrentDictionary<string, (string Otp, DateTime ExpiresAt)> _otpStore
            = new();
        private static readonly ConcurrentDictionary<string, (Guid UserId, string AccessToken, DateTime ExpiresAt)> _sessions
            = new();

        // Session lifetime in minutes
        private const int SessionMinutes = 5;

        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private ApplicationDbContext ctx;

        public AuthService(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public AuthService(ApplicationDbContext ctx)
        {
            this.ctx = ctx;
        }

        private string GenerateJwtToken(Core.Entities.User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.PhoneNumber ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(SessionMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<string> SendOtpAsync(string phoneNumber)
        {
            // Generate 6-digit numeric OTP
            var rng = Random.Shared;
            var otp = rng.Next(100000, 999999).ToString();

            var expiresAt = DateTime.UtcNow.AddMinutes(5);

            _otpStore.AddOrUpdate(phoneNumber, (otp, expiresAt), (_, __) => (otp, expiresAt));

            // In a real implementation send the OTP via SMS here..
            return Task.FromResult(otp);
        }

        public async Task<AuthResponse> VerifyOtpAsync(string phoneNumber, string otp)
        {
            if (!_otpStore.TryGetValue(phoneNumber, out var entry))
            {
                throw new InvalidOperationException("OTP not found or expired.");
            }

            if (entry.ExpiresAt < DateTime.UtcNow)
            {
                _otpStore.TryRemove(phoneNumber, out _);
                throw new InvalidOperationException("OTP expired.");
            }

            if (!string.Equals(entry.Otp, otp, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Invalid OTP.");
            }

            // OTP validated; remove it
            _otpStore.TryRemove(phoneNumber, out _);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    PhoneNumber = phoneNumber,
                    Name = "New User",
                    CreatedAt = DateTime.UtcNow
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }

            // Issue JWT access token and a refresh token
            var accessToken = GenerateJwtToken(user);
            var refreshToken = Guid.NewGuid().ToString(); // rotate/secure this in prod
            var expiresAt = DateTime.UtcNow.AddMinutes(SessionMinutes);

            _sessions[refreshToken] = (user.Id, accessToken, expiresAt);

            var userDto = new UserDto(user.Id, user.PhoneNumber, user.Name);
            return new AuthResponse(accessToken, refreshToken, userDto);
        }

        public Task LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Missing refresh token.", nameof(refreshToken));

            _sessions.TryRemove(refreshToken, out _);
            return Task.CompletedTask;
        }

        public Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Missing refresh token.", nameof(refreshToken));

            if (!_sessions.TryGetValue(refreshToken, out var session))
                throw new InvalidOperationException("Invalid refresh token.");

            // Check expiry
            if (session.ExpiresAt < DateTime.UtcNow)
            {
                // expired - remove and fail
                _sessions.TryRemove(refreshToken, out _);
                throw new InvalidOperationException("Refresh token expired.");
            }

            // rotate tokens
            var user = _db.Users.Find(session.UserId) ?? throw new InvalidOperationException("User not found.");
            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = Guid.NewGuid().ToString();
            var newExpiresAt = DateTime.UtcNow.AddMinutes(SessionMinutes);

            _sessions.TryRemove(refreshToken, out _);
            _sessions[newRefreshToken] = (session.UserId, newAccessToken, newExpiresAt);

            var userDto = new UserDto(user.Id, user.PhoneNumber, user.Name);
            return Task.FromResult(new AuthResponse(newAccessToken, newRefreshToken, userDto));
        }
    }
}