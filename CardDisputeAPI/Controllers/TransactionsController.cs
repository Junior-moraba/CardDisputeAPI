using CardDisputePortal.Core.DTOs;
using CardDisputePortal.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;


namespace CardDisputeAPI.Controllers
{
    [EnableRateLimiting("ApiPolicy")]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IMemoryCache _cache;

        public TransactionsController(ITransactionService transactionService, IMemoryCache cache)
        {
            _transactionService = transactionService;
            _cache = cache;
        }

        /// <summary>Returns a paginated list of transactions for the specified user.</summary>
        [HttpPost("list")]
        public async Task<IActionResult> GetTransactions([FromBody] GetTransactionsRequest request)
        {
            var key = $"transactions:{request.UserId}:{request.Page}:{request.Limit}:{request.SortBy}:{request.SortOrder}";
            var response = await _cache.GetOrCreateAsync(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                return _transactionService.GetTransactionsAsync(request.UserId, request.Page, request.Limit, request.SortBy, request.SortOrder);
            });
            return Ok(new { success = true, data = response });
        }

        /// <summary>Creates dummy transactions for testing purposes.</summary>
        [HttpPost("create-dummy")]
        public async Task<IActionResult> CreateDummyTransactions([FromBody] CreateDummyTransactionsRequest request)
        {
            var transactions = await _transactionService.CreateDummyTransactionsAsync(request.UserId);
            return Ok(new { success = true, message = $"Created {transactions.Count} dummy transactions", data = transactions });
        }


        /// <summary>Returns a single transaction by ID.</summary>
        [HttpGet("{id}")]
        [ResponseCache(Duration = 60, VaryByHeader = "Authorization")]
        public async Task<IActionResult> GetTransaction(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst("userId")!.Value);
            var transaction = await _transactionService.GetTransactionByIdAsync(userId, id);
            return Ok(new { success = true, data = transaction });
        }
    }
}