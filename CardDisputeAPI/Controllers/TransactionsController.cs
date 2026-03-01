using CardDisputePortal.Core.DTOs;
using CardDisputePortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CardDisputeAPI.Controllers
{
    [EnableRateLimiting("ApiPolicy")]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetTransactions([FromBody] GetTransactionsRequest request)
        {
            var response = await _transactionService.GetTransactionsAsync(request.UserId, request.Page, request.Limit, request.SortBy, request.SortOrder);
            return Ok(new { success = true, data = response });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst("userId")!.Value);
            var transaction = await _transactionService.GetTransactionByIdAsync(userId, id);
            return Ok(new { success = true, data = transaction });
        }
    }
}