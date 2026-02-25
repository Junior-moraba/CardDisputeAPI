using CardDisputePortal.Core.DTOs;
using CardDisputePortal.Core.Entities;
using CardDisputePortal.Core.Interfaces;
using CardDisputePortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CardDisputePortal.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        // Simple in-memory OTP store: phone -> (otp, expiresAt)
        private static readonly ConcurrentDictionary<string, (string Otp, DateTime ExpiresAt)> _otpStore
            = new();

        private readonly ApplicationDbContext _db;

        public AuthService(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<string> SendOtpAsync(string phoneNumber)
        {
            // Generate 6-digit numeric OTP
            var rng = Random.Shared;
            var otp = rng.Next(100000, 999999).ToString();

            var expiresAt = DateTime.UtcNow.AddMinutes(5);

            _otpStore.AddOrUpdate(phoneNumber, (otp, expiresAt), (_, __) => (otp, expiresAt));

            // In a real implementation you'd send the OTP via SMS here.
            // For development we simply return it.
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

            // Find or create user by phone number
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

            // Return dummy tokens (replace with real JWT generation)
            var accessToken = Guid.NewGuid().ToString();
            var refreshToken = Guid.NewGuid().ToString();

            var userDto = new UserDto(user.Id, user.PhoneNumber, user.Name);
            return new AuthResponse(accessToken, refreshToken, userDto);
        }
    }
}