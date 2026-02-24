using System;
using System.Collections.Generic;
using System.Text;

namespace CardDisputePortal.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
    }
}
