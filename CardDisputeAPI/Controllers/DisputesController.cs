using CardDisputePortal.Core.DTOs;
using CardDisputePortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardDisputeAPI.Controllers
{
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
        public async Task<IActionResult> CreateDispute([FromBody] CreateDisputeRequest request)
        {
            var userId = Guid.Parse(User.FindFirst("userId")!.Value);
            var dispute = await _disputeService.CreateDisputeAsync(userId, request);
            return CreatedAtAction(nameof(GetDispute), new { id = dispute.Id }, new { success = true, data = dispute });
        }


        [HttpGet]
        public async Task<IActionResult> GetDisputes([FromQuery] int page = 1, [FromQuery] int limit = 5)
        {
            var userId = Guid.Parse(User.FindFirst("userId")!.Value);
            var disputes = await _disputeService.GetDisputesAsync(userId, page, limit);
            return Ok(new { success = true, data = disputes });
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
