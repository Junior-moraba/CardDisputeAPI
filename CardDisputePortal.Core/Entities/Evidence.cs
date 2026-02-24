using System;
using System.Collections.Generic;
using System.Text;

namespace CardDisputePortal.Core.Entities
{
    public class Evidence
    {
        public Guid Id { get; set; }
        public Guid DisputeId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public Dispute Dispute { get; set; } = null!;
    }
}
