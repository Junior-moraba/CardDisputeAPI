using CardDisputePortal.Core.Entities;
using CardDisputePortal.Core.Enums;
using CardDisputePortal.Infrastructure.Data;
using CardDisputePortal.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

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

        [Fact]
        public async Task CreateDispute_TransactionNotFound_Throws()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            var svc = new DisputeService(ctx);
            var req = new CardDisputePortal.Core.DTOs.CreateDisputeRequest(userId, Guid.NewGuid(), DisputeReason.Fraudulent, "details", false);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await svc.CreateDisputeAsync(userId, req);
            });
        }

        [Fact]
        public async Task CreateDispute_WrongUser_Throws()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var tx = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId1,
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
            var req = new CardDisputePortal.Core.DTOs.CreateDisputeRequest(userId2, tx.Id, DisputeReason.Fraudulent, "details", false);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await svc.CreateDisputeAsync(userId2, req);
            });
        }

        [Fact]
        public async Task CreateDispute_SetsEstimatedResolutionDate()
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
            var req = new CardDisputePortal.Core.DTOs.CreateDisputeRequest(userId, tx.Id, DisputeReason.Fraudulent, "details", false);

            var dto = await svc.CreateDisputeAsync(userId, req);

            Assert.True(dto.EstimatedResolutionDate > dto.SubmittedAt);
            Assert.True((dto.EstimatedResolutionDate - dto.SubmittedAt).TotalDays >= 14);
        }

        [Fact]
        public async Task GetDisputes_EmptyResult_ReturnsEmptyList()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            var svc = new DisputeService(ctx);
            var resp = await svc.GetDisputesAsync(userId, page: 1, limit: 10);

            Assert.Equal(0, resp.TotalCount);
            Assert.Equal(0, resp.ReturnedCount);
            Assert.Empty(resp.Items);
        }

        [Fact]
        public async Task GetDisputes_SortByStatusAsc_ReturnsSortedResults()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            var tx1 = new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M1", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r1" };
            var tx2 = new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M2", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r2" };
            var tx3 = new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M3", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r3" };

            ctx.Transactions.AddRange(tx1, tx2, tx3);
            ctx.Disputes.AddRange(
                new Dispute { Id = Guid.NewGuid(), TransactionId = tx1.Id, UserId = userId, ReasonCode = DisputeReason.Fraudulent, Details = "d1", Status = DisputeStatus.Resolved, SubmittedAt = DateTime.UtcNow, EstimatedResolutionDate = DateTime.UtcNow.AddDays(14), Transaction = tx1 },
                new Dispute { Id = Guid.NewGuid(), TransactionId = tx2.Id, UserId = userId, ReasonCode = DisputeReason.Fraudulent, Details = "d2", Status = DisputeStatus.Pending, SubmittedAt = DateTime.UtcNow, EstimatedResolutionDate = DateTime.UtcNow.AddDays(14), Transaction = tx2 },
                new Dispute { Id = Guid.NewGuid(), TransactionId = tx3.Id, UserId = userId, ReasonCode = DisputeReason.Fraudulent, Details = "d3", Status = DisputeStatus.UnderReview, SubmittedAt = DateTime.UtcNow, EstimatedResolutionDate = DateTime.UtcNow.AddDays(14), Transaction = tx3 }
            );
            await ctx.SaveChangesAsync();

            var svc = new DisputeService(ctx);
            var resp = await svc.GetDisputesAsync(userId, page: 1, limit: 10, sortBy: "status", sortOrder: "asc");

            Assert.Equal("Pending", resp.Items[0].Status);
            Assert.Equal("UnderReview", resp.Items[1].Status);
            Assert.Equal("Resolved", resp.Items[2].Status);
        }

        [Fact]
        public async Task GetDisputes_SortByStatusDesc_ReturnsSortedResults()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            var tx1 = new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M1", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r1" };
            var tx2 = new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M2", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r2" };

            ctx.Transactions.AddRange(tx1, tx2);
            ctx.Disputes.AddRange(
                new Dispute { Id = Guid.NewGuid(), TransactionId = tx1.Id, UserId = userId, ReasonCode = DisputeReason.Fraudulent, Details = "d1", Status = DisputeStatus.Pending, SubmittedAt = DateTime.UtcNow, EstimatedResolutionDate = DateTime.UtcNow.AddDays(14), Transaction = tx1 },
                new Dispute { Id = Guid.NewGuid(), TransactionId = tx2.Id, UserId = userId, ReasonCode = DisputeReason.Fraudulent, Details = "d2", Status = DisputeStatus.Resolved, SubmittedAt = DateTime.UtcNow, EstimatedResolutionDate = DateTime.UtcNow.AddDays(14), Transaction = tx2 }
            );
            await ctx.SaveChangesAsync();

            var svc = new DisputeService(ctx);
            var resp = await svc.GetDisputesAsync(userId, page: 1, limit: 10, sortBy: "status", sortOrder: "desc");

            Assert.Equal("Resolved", resp.Items[0].Status);
            Assert.Equal("Pending", resp.Items[1].Status);
        }

        [Fact]
        public async Task GetDisputes_SortByDateAsc_ReturnsSortedResults()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            var date1 = DateTime.UtcNow.AddDays(-2);
            var date2 = DateTime.UtcNow.AddDays(-1);
            var date3 = DateTime.UtcNow;

            var tx1 = new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M1", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r1" };
            var tx2 = new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M2", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r2" };
            var tx3 = new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M3", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r3" };

            ctx.Transactions.AddRange(tx1, tx2, tx3);
            ctx.Disputes.AddRange(
                new Dispute { Id = Guid.NewGuid(), TransactionId = tx1.Id, UserId = userId, ReasonCode = DisputeReason.Fraudulent, Details = "d1", Status = DisputeStatus.Pending, SubmittedAt = date2, EstimatedResolutionDate = DateTime.UtcNow.AddDays(14), Transaction = tx1 },
                new Dispute { Id = Guid.NewGuid(), TransactionId = tx2.Id, UserId = userId, ReasonCode = DisputeReason.Fraudulent, Details = "d2", Status = DisputeStatus.Pending, SubmittedAt = date3, EstimatedResolutionDate = DateTime.UtcNow.AddDays(14), Transaction = tx2 },
                new Dispute { Id = Guid.NewGuid(), TransactionId = tx3.Id, UserId = userId, ReasonCode = DisputeReason.Fraudulent, Details = "d3", Status = DisputeStatus.Pending, SubmittedAt = date1, EstimatedResolutionDate = DateTime.UtcNow.AddDays(14), Transaction = tx3 }
            );
            await ctx.SaveChangesAsync();

            var svc = new DisputeService(ctx);
            var resp = await svc.GetDisputesAsync(userId, page: 1, limit: 10, sortBy: "date", sortOrder: "asc");

            Assert.Equal(date1, resp.Items[0].SubmittedAt);
            Assert.Equal(date2, resp.Items[1].SubmittedAt);
            Assert.Equal(date3, resp.Items[2].SubmittedAt);
        }

        [Fact]
        public async Task GetDisputes_InvalidPageNumber_NormalizesToOne()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            var svc = new DisputeService(ctx);
            var resp = await svc.GetDisputesAsync(userId, page: 0, limit: 10);

            Assert.Equal(1, resp.Page);
        }

        [Fact]
        public async Task GetDisputes_InvalidLimit_NormalizesToTen()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            var tx = new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M1", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r1" };
            ctx.Transactions.Add(tx);
            ctx.Disputes.Add(new Dispute { Id = Guid.NewGuid(), TransactionId = tx.Id, UserId = userId, ReasonCode = DisputeReason.Fraudulent, Details = "d1", Status = DisputeStatus.Pending, SubmittedAt = DateTime.UtcNow, EstimatedResolutionDate = DateTime.UtcNow.AddDays(14), Transaction = tx });
            await ctx.SaveChangesAsync();

            var svc = new DisputeService(ctx);
            var resp = await svc.GetDisputesAsync(userId, page: 1, limit: 0);

            Assert.Equal(1, resp.ReturnedCount);
        }

     
        

     }
}
