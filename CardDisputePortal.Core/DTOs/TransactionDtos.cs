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
}
