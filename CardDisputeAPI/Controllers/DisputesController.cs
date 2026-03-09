using CardDisputePortal.Core.DTOs;
using CardDisputePortal.Core.Enums;
using CardDisputePortal.Core.Interfaces;
using CardDisputePortal.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace CardDisputeAPI.Controllers
{
    [EnableRateLimiting("ApiPolicy")]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DisputesController : Controller
    {
        private readonly IDisputeService _disputeService;
        private readonly IMemoryCache _cache;

        public DisputesController(IDisputeService disputeService, IMemoryCache cache)
        {
            _disputeService = disputeService;
            _cache = cache;
        }           

        /// <summary>Creates a new dispute for a transaction.</summary>
        [HttpPost]
        // [Consumes("application/json", "multipart/form-data")]
        public async Task<IActionResult> CreateDispute()
        {
            CreateDisputeRequest request;

            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();

                if (!Guid.TryParse(form["UserId"], out var userId) ||
                    !Guid.TryParse(form["TransactionId"], out var transactionId))
                {
                    return BadRequest(new { success = false, message = "Invalid UserId or TransactionId." });
                }

                var reasonStr = form["ReasonCode"].ToString();
                if (!Enum.TryParse<DisputeReason>(reasonStr, true, out var reason))
                {
                    return BadRequest(new { success = false, message = "Invalid ReasonCode." });
                }

                var details = form["Details"].ToString();
                var evidenceAttached = bool.TryParse(form["EvidenceAttached"], out var ea) && ea;


                request = new CreateDisputeRequest(userId, transactionId, reason, details, evidenceAttached);
            }
            else
            {
                using var sr = new StreamReader(Request.Body);
                var body = await sr.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(body))
                    return BadRequest(new { success = false, message = "Empty body." });

                request = JsonSerializer.Deserialize<CreateDisputeRequest>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new InvalidOperationException("Failed to deserialize request.");
            }

            var dispute = await _disputeService.CreateDisputeAsync(request.UserId, request);
            return CreatedAtAction(nameof(GetDispute), new { id = dispute.Id }, new { success = true, data = dispute });
        }

        /// <summary>Returns a paginated list of disputes for the specified user.</summary>
        [HttpPost("list")]
        public async Task<IActionResult> GetDisputes([FromBody] GetDisputesRequest request)
        {
            var key = $"disputes:{request.UserId}:{request.Page}:{request.Limit}:{request.SortBy}:{request.SortOrder}";
            var response = await _cache.GetOrCreateAsync(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                return _disputeService.GetDisputesAsync(request.UserId, request.Page, request.Limit, request.SortBy, request.SortOrder);
            });
            return Ok(new { success = true, data = response });
        }

        /// <summary>Returns a single dispute by ID.</summary>
        [HttpGet("{id}")]
        [ResponseCache(Duration = 60, VaryByHeader = "Authorization")]
        public async Task<IActionResult> GetDispute(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst("userId")!.Value);
            var dispute = await _disputeService.GetDisputeByIdAsync(userId, id);
            return Ok(new { success = true, data = dispute });
        }
    }
}