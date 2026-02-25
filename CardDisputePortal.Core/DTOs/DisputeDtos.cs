using System;
using CardDisputePortal.Core.Enums;

namespace CardDisputePortal.Core.DTOs
{
    public record CreateDisputeRequest(
        Guid UserId,
        Guid TransactionId,
        DisputeReason ReasonCode,
        string Details,
        bool EvidenceAttached
    );

    // Form-based request (ReasonCode as string because form-data sends text)
    public record CreateDisputeFormRequest(
        Guid UserId,
        Guid TransactionId,
        string ReasonCode,
        string Details,
        bool EvidenceAttached
    );

    public record DisputeDto(
        Guid Id,
        Guid TransactionId,
        DisputeReason ReasonCode,
        string Details,
        bool EvidenceAttached,
        string Status,
        DateTime SubmittedAt,
        DateTime EstimatedResolutionDate
    );

    // Request DTO for listing disputes (accept userId in body)
    public record GetDisputesRequest(Guid UserId, int Page = 1, int Limit = 5);
}