using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using CardDisputePortal.Core.Enums;

namespace CardDisputePortal.Core.Entities
{
    public class Dispute
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }

        public DisputeReason ReasonCode { get; set; } = DisputeReason.Other;

        public string Details { get; set; } = string.Empty;
        public bool EvidenceAttached { get; set; }
        public DisputeStatus Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime EstimatedResolutionDate { get; set; }
        public Transaction Transaction { get; set; } = null!;
        public User User { get; set; } = null!;
        public ICollection<Evidence> EvidenceFiles { get; set; } = new List<Evidence>();
    }

    public enum DisputeStatus
    {
        Pending,
        UnderReview,
        Resolved,
        Rejected
    }
}
