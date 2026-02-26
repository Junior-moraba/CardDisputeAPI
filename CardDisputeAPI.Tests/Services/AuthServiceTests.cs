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
            Assert.Equal(auth.AccessToken, newAuth.AccessToken);
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

        [Fact]
        public async Task VerifyOtp_ExistingUser_ReturnsTokens()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);
            var phone = $"+100000{new Random().Next(1000, 9999)}";

            // First login - creates user
            var otp1 = await svc.SendOtpAsync(phone);
            var auth1 = await svc.VerifyOtpAsync(phone, otp1);

            // Second login - uses existing user
            var otp2 = await svc.SendOtpAsync(phone);
            var auth2 = await svc.VerifyOtpAsync(phone, otp2);

            Assert.Equal(auth1.User.Id, auth2.User.Id);
            Assert.NotEqual(auth1.RefreshToken, auth2.RefreshToken);
        }

        [Fact]
        public async Task VerifyOtp_InvalidOtp_Throws()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);
            var phone = $"+100000{new Random().Next(1000, 9999)}";

            await svc.SendOtpAsync(phone);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await svc.VerifyOtpAsync(phone, "000000");
            });
        }

        [Fact]
        public async Task VerifyOtp_NonExistentPhone_Throws()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await svc.VerifyOtpAsync("+1234567890", "123456");
            });
        }

        [Fact]
        public async Task VerifyOtp_ExpiredOtp_Throws()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);
            var phone = $"+100000{new Random().Next(1000, 9999)}";

            var otp = await svc.SendOtpAsync(phone);

            var otpStoreField = typeof(AuthService).GetField("_otpStore", BindingFlags.NonPublic | BindingFlags.Static);
            var otpStore = otpStoreField.GetValue(null) as ConcurrentDictionary<string, (string Otp, DateTime ExpiresAt)>;

            if (otpStore.TryGetValue(phone, out var entry))
            {
                otpStore[phone] = (entry.Otp, DateTime.UtcNow.AddMinutes(-1));
            }

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await svc.VerifyOtpAsync(phone, otp);
            });
        }

        [Fact]
        public async Task RefreshToken_InvalidToken_Throws()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await svc.RefreshTokenAsync("invalid-token");
            });
        }

        [Fact]
        public async Task RefreshToken_EmptyToken_Throws()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await svc.RefreshTokenAsync("");
            });
        }

        [Fact]
        public async Task Logout_EmptyToken_Throws()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await svc.LogoutAsync("");
            });
        }

        [Fact]
        public async Task SendOtp_GeneratesValidOtp()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);
            var phone = $"+100000{new Random().Next(1000, 9999)}";

            var otp = await svc.SendOtpAsync(phone);

            Assert.NotNull(otp);
            Assert.Equal(6, otp.Length);
            Assert.True(int.TryParse(otp, out var otpNum));
            Assert.InRange(otpNum, 100000, 999999);
        }

        [Fact]
        public async Task SendOtp_OverwritesPreviousOtp()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);
            var phone = $"+100000{new Random().Next(1000, 9999)}";

            var otp1 = await svc.SendOtpAsync(phone);
            var otp2 = await svc.SendOtpAsync(phone);

            // First OTP should no longer work
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await svc.VerifyOtpAsync(phone, otp1);
            });

            // Second OTP should work
            var auth = await svc.VerifyOtpAsync(phone, otp2);
            Assert.NotNull(auth);
        }

        [Fact]
        public async Task RefreshToken_OldTokenInvalidated()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var svc = new AuthService(ctx);
            var phone = $"+100000{new Random().Next(1000, 9999)}";

            var otp = await svc.SendOtpAsync(phone);
            var auth = await svc.VerifyOtpAsync(phone, otp);
            var oldRefreshToken = auth.RefreshToken;

            await svc.RefreshTokenAsync(oldRefreshToken);

            // Old refresh token should no longer work
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await svc.RefreshTokenAsync(oldRefreshToken);
            });
        }

    }
}
