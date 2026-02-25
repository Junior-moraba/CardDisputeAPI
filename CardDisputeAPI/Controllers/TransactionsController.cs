using CardDisputePortal.Core.Entities;
using CardDisputePortal.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CardDisputeAPI.Controllers
{
    public class TransactionsController : ControllerBase
    {

        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int limit = 5)
        {
            var userId = Guid.Parse(User.FindFirst("userId")!.Value);
            var transactions = await _transactionService.GetTransactionsAsync(userId, page, limit);
            return Ok(new { success = true, data = transactions });
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