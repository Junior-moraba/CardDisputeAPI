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

    // Request DTO for listing transactions (accept userId in body)
    public record GetTransactionsRequest(Guid UserId, int Page = 1, int Limit = 5, string SortBy = "date", string SortOrder = "desc");

    // Response wrapper including pagination metadata
    public record PaginatedTransactionsResponse(
        int Page,
        int ReturnedCount,
        int TotalCount,
        int TotalPages,
        List<TransactionDto> Items
    );

    public record CreateDummyTransactionsRequest(Guid UserId);
}