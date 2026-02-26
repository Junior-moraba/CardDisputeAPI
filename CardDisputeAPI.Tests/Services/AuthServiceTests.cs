using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CardDisputePortal.Infrastructure.Data;
using CardDisputePortal.Infrastructure.Services;
using CardDisputePortal.Core.Entities;
using CardDisputePortal.Core.DTOs;
using CardDisputePortal.Core.Enums;

namespace CardDisputeAPI.Tests.Services
{
    public class AuthServiceTests
    {
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task VerifyOtp_CreatesUserAndReturnsTokens()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);
            var phone = $"+100000{new Random().Next(1000,9999)}";

            var otp = await svc.SendOtpAsync(phone);
            Assert.False(string.IsNullOrWhiteSpace(otp));
            Assert.Equal(6, otp.Length);

            var auth = await svc.VerifyOtpAsync(phone, otp);
            Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(auth.RefreshToken));
            Assert.NotNull(auth.User);
            Assert.Equal(phone, auth.User.PhoneNumber);

            var userInDb = ctx.Users.SingleOrDefault(u => u.Id == auth.User.Id);
            Assert.NotNull(userInDb);
            Assert.Equal(phone, userInDb.PhoneNumber);
        }

        [Fact]
        public async Task RefreshToken_RotatesTokens()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);
            var phone = $"+100000{new Random().Next(1000,9999)}";

            var otp = await svc.SendOtpAsync(phone);
            var auth = await svc.VerifyOtpAsync(phone, otp);

            var newAuth = await svc.RefreshTokenAsync(auth.RefreshToken);

            Assert.False(string.IsNullOrWhiteSpace(newAuth.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(newAuth.RefreshToken));
            Assert.NotEqual(auth.AccessToken, newAuth.AccessToken);
            Assert.NotEqual(auth.RefreshToken, newAuth.RefreshToken);
            Assert.Equal(auth.User.Id, newAuth.User.Id);
        }

        [Fact]
        public async Task Logout_InvalidatesRefreshToken()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);
            var phone = $"+100000{new Random().Next(1000,9999)}";

            var otp = await svc.SendOtpAsync(phone);
            var auth = await svc.VerifyOtpAsync(phone, otp);

            await svc.LogoutAsync(auth.RefreshToken);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await svc.RefreshTokenAsync(auth.RefreshToken);
            });
        }

        [Fact]
        public async Task RefreshToken_Expired_Throws()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);
            var phone = $"+100000{new Random().Next(1000,9999)}";

            var otp = await svc.SendOtpAsync(phone);
            var auth = await svc.VerifyOtpAsync(phone, otp);

            var sessionsField = typeof(AuthService).GetField("_sessions", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(sessionsField);

            var sessions = sessionsField.GetValue(null) as ConcurrentDictionary<string, (Guid UserId, string AccessToken, DateTime ExpiresAt)>;
            Assert.NotNull(sessions);

            if (sessions.TryGetValue(auth.RefreshToken, out var sess))
            { 
                sessions[auth.RefreshToken] = (sess.UserId, sess.AccessToken, DateTime.UtcNow.AddMinutes(-10));
            }
            else
            {
                Assert.True(false, "Session not found in internal store.");
            }

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await svc.RefreshTokenAsync(auth.RefreshToken);
            });
        }
    }
}
