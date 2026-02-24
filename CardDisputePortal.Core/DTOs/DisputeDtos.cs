using System;
using System.Collections.Generic;
using System.Text;

namespace CardDisputePortal.Core.DTOs
{
    public record CreateDisputeRequest(
        Guid TransactionId,
        string ReasonCode,
        string Details,
        bool EvidenceAttached
    );

    public record DisputeDto(
        Guid Id,
        Guid TransactionId,
        string ReasonCode,
        string Details,
        bool EvidenceAttached,
        string Status,
        DateTime SubmittedAt,
        DateTime EstimatedResolutionDate
    );
}
