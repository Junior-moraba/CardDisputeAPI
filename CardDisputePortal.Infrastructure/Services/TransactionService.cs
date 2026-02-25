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

        public async Task<PaginatedTransactionsResponse> GetTransactionsAsync(Guid userId, int page, int limit)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 5;

            var skip = (page - 1) * limit;

            var query = _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.Date)
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

            return new PaginatedTransactionsResponse(
                Page: page,
                ReturnedCount: items.Count,
                TotalCount: totalCount,
                Items: items
            );
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