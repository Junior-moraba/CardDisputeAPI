using CardDisputePortal.Core.DTOs;
using CardDisputePortal.Core.Interfaces;
using CardDisputePortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardDisputePortal.Infrastructure.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _db;

        public TransactionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PaginatedTransactionsResponse> GetTransactionsAsync(Guid userId, int page, int limit, string sortBy = "date", string sortOrder = "desc")
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 5;

            var skip = (page - 1) * limit;

            var baseQuery = _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId);

            var totalCount = await baseQuery.CountAsync();

            // normalize sort values
            var sortByLower = (sortBy ?? "date").Trim().ToLowerInvariant();
            var sortOrderLower = (sortOrder ?? "desc").Trim().ToLowerInvariant();

            IQueryable<Core.Entities.Transaction> orderedQuery;

            if (sortByLower == "amount")
            {
                orderedQuery = sortOrderLower == "asc"
                    ? baseQuery.OrderBy(t => t.Amount)
                    : baseQuery.OrderByDescending(t => t.Amount);
            }
            else // default sort by date
            {
                orderedQuery = sortOrderLower == "asc"
                    ? baseQuery.OrderBy(t => t.Date)
                    : baseQuery.OrderByDescending(t => t.Date);
            }

            var items = await orderedQuery
                .Skip(skip)
                .Take(limit)
                .Select(t => new TransactionDto(
                    t.Id,
                    t.Date,
                    new MerchantDto(t.MerchantName, t.MerchantCategory),
                    t.Amount,
                    t.Currency,
                    t.Status.ToString(),
                    t.Reference
                ))
                .ToListAsync();

            var totalPages = limit > 0 ? (int)Math.Ceiling((double)totalCount / limit) : 0;

            return new PaginatedTransactionsResponse(
                Page: page,
                ReturnedCount: items.Count,
                TotalCount: totalCount,
                TotalPages: totalPages,
                Items: items
            );
        }

        public async Task<List<TransactionDto>> CreateDummyTransactionsAsync(Guid userId)
        {
            var southAfricanTransactions = new[]
            {
                new { Merchant = "Woolworths", Category = "Groceries", Amount = 1245.67m },
                new { Merchant = "Capitec Bank ATM", Category = "Banking", Amount = 500.00m },
                new { Merchant = "Sasol", Category = "Fuel", Amount = 850.00m },
                new { Merchant = "Checkers", Category = "Groceries", Amount = 678.90m },
                new { Merchant = "Dis-Chem", Category = "Health & Beauty", Amount = 234.50m },
                new { Merchant = "Mugg & Bean", Category = "Food & Beverage", Amount = 189.00m },
                new { Merchant = "Edgars", Category = "Retail", Amount = 1899.99m },
                new { Merchant = "Spur", Category = "Food & Beverage", Amount = 456.80m },
                new { Merchant = "BP", Category = "Fuel", Amount = 720.00m },
                new { Merchant = "Game", Category = "Electronics", Amount = 2999.99m },
                new { Merchant = "Vida e Caffè", Category = "Food & Beverage", Amount = 67.50m }
            };

            var transactions = new List<Core.Entities.Transaction>();
            var random = new Random();

            for (int i = 0; i < southAfricanTransactions.Length; i++)
            {
                var txn = southAfricanTransactions[i];
                var transaction = new Core.Entities.Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Amount = txn.Amount,
                    MerchantName = txn.Merchant,
                    MerchantCategory = txn.Category,
                    Date = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                    Status = (CardDisputePortal.Core.Entities.TransactionStatus)random.Next(0, 2),
                    Reference = $"TXN{DateTime.Now.Ticks.ToString().Substring(10)}{i:D3}",
                    Currency = "ZAR"
                };
                transactions.Add(transaction);
            }

            _db.Transactions.AddRange(transactions);
            await _db.SaveChangesAsync();

            return transactions.Select(t => new TransactionDto(
                t.Id,
                t.Date,
                new MerchantDto(t.MerchantName, t.MerchantCategory),
                t.Amount,
                t.Currency,
                t.Status.ToString(),
                t.Reference
            )).ToList();
        }


        public async Task<TransactionDto> GetTransactionByIdAsync(Guid userId, Guid transactionId)
        {
            var dto = await _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Id == transactionId)
                .Select(t => new TransactionDto(
                    t.Id,
                    t.Date,
                    new MerchantDto(t.MerchantName, t.MerchantCategory),
                    t.Amount,
                    t.Currency,
                    t.Status.ToString(),
                    t.Reference
                ))
                .FirstOrDefaultAsync();

            if (dto is null)
            {
                throw new KeyNotFoundException("Transaction not found.");
            }

            return dto;
        }
    }
}