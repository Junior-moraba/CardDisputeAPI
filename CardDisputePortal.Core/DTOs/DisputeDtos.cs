using System;
using System.Collections.Generic;
using System.Text;
using CardDisputePortal.Core.Enums;

namespace CardDisputePortal.Core.DTOs
{
    public record CreateDisputeRequest(
        Guid TransactionId,
        DisputeReason ReasonCode,
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

    public record GetDisputesRequest(Guid UserId, int Page = 1, int Limit = 5);
}
