using CardDisputePortal.Core.DTOs;
using CardDisputePortal.Core.Enums;
using CardDisputePortal.Core.Interfaces;
using CardDisputePortal.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;

namespace CardDisputeAPI.Controllers
{
    [EnableRateLimiting("ApiPolicy")]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DisputesController : Controller
    {
        private readonly IDisputeService _disputeService;

        public DisputesController(IDisputeService disputeService)
        {
            _disputeService = disputeService;
        }           

        [HttpPost]
        [Consumes("application/json", "multipart/form-data")]
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

        [HttpPost("list")]
        public async Task<IActionResult> GetDisputes([FromBody] GetDisputesRequest request)
        {
            var response = await _disputeService.GetDisputesAsync(request.UserId, request.Page, request.Limit, request.SortBy, request.SortOrder);
            return Ok(new { success = true, data = response });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDispute(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst("userId")!.Value);
            var dispute = await _disputeService.GetDisputeByIdAsync(userId, id);
            return Ok(new { success = true, data = dispute });
        }
    }
}