using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CardDisputePortal.Infrastructure.Data;
using CardDisputePortal.Infrastructure.Services;
using CardDisputePortal.Core.Entities;
using CardDisputePortal.Core.Enums;

namespace CardDisputeAPI.Tests.Services
{
    public class DisputeServiceTests
    {
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetDisputes_IncludesMerchantAndReference_AndReturnsPagination()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();

            // create transactions and disputes
            for (int i = 0; i < 3; i++)
            {
                var tx = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Date = DateTime.UtcNow.AddDays(i),
                    MerchantName = $"Merchant{i}",
                    MerchantCategory = "cat",
                    Amount = i + 10,
                    Currency = "ZAR",
                    Status = TransactionStatus.Completed,
                    Reference = $"REF-{i}"
                };
                ctx.Transactions.Add(tx);

                ctx.Disputes.Add(new Dispute
                {
                    Id = Guid.NewGuid(),
                    TransactionId = tx.Id,
                    UserId = userId,
                    ReasonCode = DisputeReason.Duplicate,
                    Details = $"details {i}",
                    EvidenceAttached = false,
                    Status = DisputeStatus.Pending,
                    SubmittedAt = DateTime.UtcNow.AddDays(i),
                    EstimatedResolutionDate = DateTime.UtcNow.AddDays(i + 7),
                    Transaction = tx
                });
            }

            await ctx.SaveChangesAsync();

            var svc = new DisputeService(ctx);
            var resp = await svc.GetDisputesAsync(userId, page: 1, limit: 10);

            Assert.Equal(1, resp.Page);
            Assert.Equal(3, resp.ReturnedCount);
            Assert.Equal(3, resp.TotalCount);
            Assert.Equal(1, resp.TotalPages);
            Assert.Equal(3, resp.Items.Count);

            var first = resp.Items.First();
            Assert.NotNull(first.Merchant);
            Assert.False(string.IsNullOrWhiteSpace(first.Reference));
            Assert.False(string.IsNullOrWhiteSpace(first.ReasonCode));
        }

        [Fact]
        public async Task CreateDispute_CreatesAndMarksTransactionAsDisputed()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            var tx = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = DateTime.UtcNow,
                MerchantName = "Store",
                MerchantCategory = "Retail",
                Amount = 50m,
                Currency = "ZAR",
                Status = TransactionStatus.Completed,
                Reference = "REF-123"
            };
            ctx.Transactions.Add(tx);
            await ctx.SaveChangesAsync();

            var svc = new DisputeService(ctx);
            var req = new CardDisputePortal.Core.DTOs.CreateDisputeRequest(userId, tx.Id, DisputeReason.Fraudulent, "I did not authorize", false);

            var dto = await svc.CreateDisputeAsync(userId, req);

            Assert.NotNull(dto);
            Assert.Equal(tx.Id, dto.TransactionId);
            Assert.Equal("Store", dto.Merchant.Name);
            Assert.Equal("REF-123", dto.Reference);
            Assert.Equal("Pending", dto.Status);

            // verify transaction updated
            var txFromDb = await ctx.Transactions.FindAsync(tx.Id);
            Assert.Equal(TransactionStatus.Disputed, txFromDb.Status);

            // ensure dispute persisted
            var created = ctx.Disputes.SingleOrDefault(d => d.Id == dto.Id);
            Assert.NotNull(created);
            Assert.Equal(DisputeReason.Fraudulent, created.ReasonCode);
        }
    }
}
