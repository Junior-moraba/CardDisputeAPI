using CardDisputePortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardDisputePortal.Core.Interfaces
{
    public interface ITransactionService
    {
        Task<List<TransactionDto>> GetTransactionsAsync(Guid userId, int page, int limit);
        Task<TransactionDto> GetTransactionByIdAsync(Guid userId, Guid transactionId);
    }
}
