using CardDisputePortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardDisputePortal.Core.Interfaces
{
    public interface IDisputeService
    {
        Task<DisputeDto> CreateDisputeAsync(Guid userId, CreateDisputeRequest request);
        Task<List<DisputeDto>> GetDisputesAsync(Guid userId, int page, int limit);
        Task<DisputeDto> GetDisputeByIdAsync(Guid userId, Guid disputeId);
    }
}
