using System;
using CardDisputePortal.Core.Enums;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CardDisputePortal.Core.DTOs
{
    public record CreateDisputeRequest(
        Guid UserId,
        Guid TransactionId,
        DisputeReason ReasonCode,
        string Details,
        bool EvidenceAttached
    );

    public record CreateDisputeFormRequest(
        Guid UserId,
        Guid TransactionId,
        DisputeReason ReasonCode,
        string Details,
        bool EvidenceAttached
    );

    public record DisputeDto(
        Guid Id,
        Guid TransactionId,
        MerchantDto Merchant,
        string Reference,
        string ReasonCode,
        string Details,
        bool EvidenceAttached,
        string Status,
        DateTime SubmittedAt,
        DateTime EstimatedResolutionDate
    );

    public record GetDisputesRequest(
        Guid UserId,
        [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] int Page = 1,
        [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] int Limit = 5
    );

    public record PaginatedDisputesResponse(
        int Page,
        int ReturnedCount,
        int TotalCount,
        int TotalPages,
        List<DisputeDto> Items
    );
}