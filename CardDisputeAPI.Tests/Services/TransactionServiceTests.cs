using CardDisputePortal.Core.Entities;
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
    public class TransactionServiceTests
    {
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetTransactions_ReturnsPaginatedResponse()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            // seed 7 transactions
            for (int i = 0; i < 7; i++)
            {
                ctx.Transactions.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Date = DateTime.UtcNow.AddDays(i),
                    MerchantName = $"M{i}",
                    MerchantCategory = "cat",
                    Amount = i + 1,
                    Currency = "ZAR",
                    Status = TransactionStatus.Completed,
                    Reference = $"ref-{i}"
                });
            }
            await ctx.SaveChangesAsync();

            var svc = new TransactionService(ctx);
            var resp = await svc.GetTransactionsAsync(userId, page: 1, limit: 5);

            Assert.Equal(1, resp.Page);
            Assert.Equal(5, resp.ReturnedCount);
            Assert.Equal(7, resp.TotalCount);
            Assert.Equal(2, resp.TotalPages);
            Assert.Equal(5, resp.Items.Count);

            // default sort is by date desc, ensure first item is the most recent
            var maxDate = ctx.Transactions.Where(t => t.UserId == userId).Max(t => t.Date);
            Assert.Equal(maxDate, resp.Items.First().Date);
        }

        [Fact]
        public async Task GetTransactions_EmptyResult_ReturnsEmptyList()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            var svc = new TransactionService(ctx);
            var resp = await svc.GetTransactionsAsync(userId, page: 1, limit: 5);

            Assert.Equal(0, resp.TotalCount);
            Assert.Equal(0, resp.ReturnedCount);
            Assert.Empty(resp.Items);
        }

        [Fact]
        public async Task GetTransactions_SortByAmountAsc_ReturnsSortedResults()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            ctx.Transactions.AddRange(
                new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M1", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r1" },
                new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M2", MerchantCategory = "cat", Amount = 50, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r2" },
                new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M3", MerchantCategory = "cat", Amount = 200, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r3" }
            );
            await ctx.SaveChangesAsync();

            var svc = new TransactionService(ctx);
            var resp = await svc.GetTransactionsAsync(userId, page: 1, limit: 10, sortBy: "amount", sortOrder: "asc");

            Assert.Equal(50, resp.Items[0].Amount);
            Assert.Equal(100, resp.Items[1].Amount);
            Assert.Equal(200, resp.Items[2].Amount);
        }

        [Fact]
        public async Task GetTransactions_SortByAmountDesc_ReturnsSortedResults()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            ctx.Transactions.AddRange(
                new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M1", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r1" },
                new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M2", MerchantCategory = "cat", Amount = 50, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r2" },
                new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M3", MerchantCategory = "cat", Amount = 200, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r3" }
            );
            await ctx.SaveChangesAsync();

            var svc = new TransactionService(ctx);
            var resp = await svc.GetTransactionsAsync(userId, page: 1, limit: 10, sortBy: "amount", sortOrder: "desc");

            Assert.Equal(200, resp.Items[0].Amount);
            Assert.Equal(100, resp.Items[1].Amount);
            Assert.Equal(50, resp.Items[2].Amount);
        }

        [Fact]
        public async Task GetTransactions_SortByDateAsc_ReturnsSortedResults()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            var date1 = DateTime.UtcNow.AddDays(-2);
            var date2 = DateTime.UtcNow.AddDays(-1);
            var date3 = DateTime.UtcNow;

            ctx.Transactions.AddRange(
                new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = date2, MerchantName = "M1", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r1" },
                new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = date3, MerchantName = "M2", MerchantCategory = "cat", Amount = 50, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r2" },
                new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = date1, MerchantName = "M3", MerchantCategory = "cat", Amount = 200, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r3" }
            );
            await ctx.SaveChangesAsync();

            var svc = new TransactionService(ctx);
            var resp = await svc.GetTransactionsAsync(userId, page: 1, limit: 10, sortBy: "date", sortOrder: "asc");

            Assert.Equal(date1, resp.Items[0].Date);
            Assert.Equal(date2, resp.Items[1].Date);
            Assert.Equal(date3, resp.Items[2].Date);
        }

        [Fact]
        public async Task GetTransactions_InvalidPageNumber_NormalizesToOne()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            ctx.Transactions.Add(new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M1", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r1" });
            await ctx.SaveChangesAsync();

            var svc = new TransactionService(ctx);
            var resp = await svc.GetTransactionsAsync(userId, page: 0, limit: 5);

            Assert.Equal(1, resp.Page);
        }

        [Fact]
        public async Task GetTransactions_InvalidLimit_NormalizesToFive()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            ctx.Transactions.Add(new Transaction { Id = Guid.NewGuid(), UserId = userId, Date = DateTime.UtcNow, MerchantName = "M1", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r1" });
            await ctx.SaveChangesAsync();

            var svc = new TransactionService(ctx);
            var resp = await svc.GetTransactionsAsync(userId, page: 1, limit: 0);

            Assert.Equal(1, resp.ReturnedCount);
        }

        [Fact]
        public async Task GetTransactions_SecondPage_ReturnsCorrectItems()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            for (int i = 0; i < 10; i++)
            {
                ctx.Transactions.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Date = DateTime.UtcNow.AddDays(-i),
                    MerchantName = $"M{i}",
                    MerchantCategory = "cat",
                    Amount = i + 1,
                    Currency = "ZAR",
                    Status = TransactionStatus.Completed,
                    Reference = $"ref-{i}"
                });
            }
            await ctx.SaveChangesAsync();

            var svc = new TransactionService(ctx);
            var resp = await svc.GetTransactionsAsync(userId, page: 2, limit: 3);

            Assert.Equal(2, resp.Page);
            Assert.Equal(3, resp.ReturnedCount);
            Assert.Equal(10, resp.TotalCount);
            Assert.Equal(4, resp.TotalPages);
        }

        [Fact]
        public async Task GetTransactions_FiltersByUserId()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            ctx.Transactions.AddRange(
                new Transaction { Id = Guid.NewGuid(), UserId = userId1, Date = DateTime.UtcNow, MerchantName = "M1", MerchantCategory = "cat", Amount = 100, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r1" },
                new Transaction { Id = Guid.NewGuid(), UserId = userId2, Date = DateTime.UtcNow, MerchantName = "M2", MerchantCategory = "cat", Amount = 50, Currency = "ZAR", Status = TransactionStatus.Completed, Reference = "r2" }
            );
            await ctx.SaveChangesAsync();

            var svc = new TransactionService(ctx);
            var resp = await svc.GetTransactionsAsync(userId1, page: 1, limit: 10);

            Assert.Equal(1, resp.TotalCount);
            Assert.Equal("M1", resp.Items[0].Merchant.Name);
        }

        [Fact]
        public async Task GetTransactionById_ReturnsTransaction()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            var transactionId = Guid.NewGuid();
            ctx.Transactions.Add(new Transaction
            {
                Id = transactionId,
                UserId = userId,
                Date = DateTime.UtcNow,
                MerchantName = "TestMerchant",
                MerchantCategory = "Shopping",
                Amount = 150.50m,
                Currency = "ZAR",
                Status = TransactionStatus.Completed,
                Reference = "ref-123"
            });
            await ctx.SaveChangesAsync();

            var svc = new TransactionService(ctx);
            var result = await svc.GetTransactionByIdAsync(userId, transactionId);

            Assert.Equal(transactionId, result.Id);
            Assert.Equal("TestMerchant", result.Merchant.Name);
            Assert.Equal(150.50m, result.Amount);
        }

        [Fact]
        public async Task GetTransactionById_NotFound_Throws()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId = Guid.NewGuid();
            var svc = new TransactionService(ctx);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await svc.GetTransactionByIdAsync(userId, Guid.NewGuid());
            });
        }

        [Fact]
        public async Task GetTransactionById_WrongUser_Throws()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);

            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var transactionId = Guid.NewGuid();

            ctx.Transactions.Add(new Transaction
            {
                Id = transactionId,
                UserId = userId1,
                Date = DateTime.UtcNow,
                MerchantName = "M1",
                MerchantCategory = "cat",
                Amount = 100,
                Currency = "ZAR",
                Status = TransactionStatus.Completed,
                Reference = "r1"
            });
            await ctx.SaveChangesAsync();

            var svc = new TransactionService(ctx);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await svc.GetTransactionByIdAsync(userId2, transactionId);
            });
        }

    }
}
