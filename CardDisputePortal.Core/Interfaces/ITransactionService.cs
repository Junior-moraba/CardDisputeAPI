using CardDisputePortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CardDisputePortal.Core.Interfaces
{
    public interface ITransactionService
    {
        Task<PaginatedTransactionsResponse> GetTransactionsAsync(Guid userId, int page, int limit, string sortBy = "date", string sortOrder = "desc");
        Task<TransactionDto> GetTransactionByIdAsync(Guid userId, Guid transactionId);
        Task<List<TransactionDto>> CreateDummyTransactionsAsync(Guid userId);
    }
}