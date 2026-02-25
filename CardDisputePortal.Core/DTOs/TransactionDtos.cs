using System;
using System.Collections.Generic;
using System.Text;

namespace CardDisputePortal.Core.DTOs
{
    public record TransactionDto(
        Guid Id,
        DateTime Date,
        MerchantDto Merchant,
        decimal Amount,
        string Currency,
        string Status,
        string Reference
    );

    public record MerchantDto(string Name, string Category);

    public record GetTransactionsRequest(Guid UserId, int Page = 1, int Limit = 5);

    public record PaginatedTransactionsResponse(
        int Page,
        int ReturnedCount,
        int TotalCount,
        List<TransactionDto> Items
    );
}
