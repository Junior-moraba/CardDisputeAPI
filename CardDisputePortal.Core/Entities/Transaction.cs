using System;
using System.Collections.Generic;
using System.Text;

namespace CardDisputePortal.Core.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime Date { get; set; }
        public string MerchantName { get; set; } = string.Empty;
        public string MerchantCategory { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "ZAR";
        public TransactionStatus Status { get; set; }
        public string Reference { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public Dispute? Dispute { get; set; }
    }

    public enum TransactionStatus
    {
        Completed,
        Pending,
        Disputed
    }
}
