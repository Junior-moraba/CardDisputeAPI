using CardDisputePortal.Core.DTOs;
using CardDisputePortal.Core.Interfaces;
using CardDisputePortal.Core.Entities;
using CardDisputePortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardDisputePortal.Infrastructure.Services
{
    public class DisputeService : IDisputeService
    {
        private readonly ApplicationDbContext _db;

        public DisputeService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DisputeDto> CreateDisputeAsync(Guid userId, CreateDisputeRequest request)
        {
            var transaction = await _db.Transactions
                .FirstOrDefaultAsync(t => t.Id == request.TransactionId);

            if (transaction == null || transaction.UserId != userId)
                throw new KeyNotFoundException("Transaction not found for this user.");

            var dispute = new Dispute
            {
                Id = Guid.NewGuid(),
                TransactionId = request.TransactionId,
                UserId = userId,
                ReasonCode = request.ReasonCode,
                Details = request.Details,
                EvidenceAttached = request.EvidenceAttached,
                Status = DisputeStatus.Pending,
                SubmittedAt = DateTime.UtcNow,
                EstimatedResolutionDate = DateTime.UtcNow.AddDays(14)
            };

            // Mark transaction as disputed
            transaction.Status = TransactionStatus.Disputed;

            _db.Disputes.Add(dispute);
            await _db.SaveChangesAsync();

            return new DisputeDto(
                dispute.Id,
                dispute.TransactionId,
                dispute.ReasonCode,
                dispute.Details,
                dispute.EvidenceAttached,
                dispute.Status.ToString(),
                dispute.SubmittedAt,
                dispute.EstimatedResolutionDate
            );
        }

        public async Task<List<DisputeDto>> GetDisputesAsync(Guid userId, int page, int limit)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;

            var skip = (page - 1) * limit;

            return await _db.Disputes
                .AsNoTracking()
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.SubmittedAt)
                .Skip(skip)
                .Take(limit)
                .Select(d => new DisputeDto(
                    d.Id,
                    d.TransactionId,
                    d.ReasonCode,
                    d.Details,
                    d.EvidenceAttached,
                    d.Status.ToString(),
                    d.SubmittedAt,
                    d.EstimatedResolutionDate
                ))
                .ToListAsync();
        }

        public async Task<DisputeDto> GetDisputeByIdAsync(Guid userId, Guid disputeId)
        {
            var dto = await _db.Disputes
                .AsNoTracking()
                .Where(d => d.Id == disputeId && d.UserId == userId)
                .Select(d => new DisputeDto(
                    d.Id,
                    d.TransactionId,
                    d.ReasonCode,
                    d.Details,
                    d.EvidenceAttached,
                    d.Status.ToString(),
                    d.SubmittedAt,
                    d.EstimatedResolutionDate
                ))
                .FirstOrDefaultAsync();

            if (dto is null)
                throw new KeyNotFoundException("Dispute not found.");

            return dto;
        }
    }
}