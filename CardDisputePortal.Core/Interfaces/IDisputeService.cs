using CardDisputePortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CardDisputePortal.Core.Interfaces
{
    public interface IDisputeService
    {
        Task<DisputeDto> CreateDisputeAsync(Guid userId, CreateDisputeRequest request);
        Task<PaginatedDisputesResponse> GetDisputesAsync(Guid userId, int page, int limit, string sortBy = "date", string sortOrder = "desc");
        Task<DisputeDto> GetDisputeByIdAsync(Guid userId, Guid disputeId);
    }
}