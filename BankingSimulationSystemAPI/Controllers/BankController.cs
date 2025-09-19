using BankingSimulationSystemAPI.Data;
using BankingSimulationSystemAPI.Model;
using BankingSimulationSystemAPI.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BankingSimulationSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BankController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BankController(AppDbContext context)
        {
            _context = context;
        }

        
        private string GenerateNumericAccountNumber()
        {
            Random random = new Random();
            return random.Next(1000000000, int.MaxValue).ToString(); // Always 10+ digits
        }

        //  Create Account
        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccount([FromBody] AccountRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var existingAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.UserID == userId);
            if (existingAccount != null)
                return BadRequest("Account already exists.");

            var account = new Account
            {
                UserID = userId,
                AccountNumber = GenerateNumericAccountNumber(),
                Balance = request.InitialBalance,
                Transactions = new List<Transaction>()
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new AccountResponse
            {
                AccountId = account.ID,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance
            });
        }

        // Get Account
        [HttpGet("account")]
        public async Task<IActionResult> GetAccount()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var account = await _context.Accounts
                .Include(a => a.User) 
                .FirstOrDefaultAsync(a => a.UserID == userId);

            if (account == null)
                return NotFound("Account not found.");

            return Ok(new AccountResponse
            {
                AccountId = account.ID,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                FirstName = account.User?.FirstName ?? "",
                LastName = account.User?.LastName ?? ""
            });
        }

        // ✅ Deposit
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] TransactionRequest request)
        {
            if (request.Amount <= 0) return BadRequest("Amount must be greater than zero");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserID == userId);

            if (account == null)
                return NotFound("Account not found.");

            account.Balance += request.Amount;

            _context.Transactions.Add(new Transaction
            {
                AccountId = account.ID,
                Amount = request.Amount,
                Type = TransactionType.Deposit,
                Description = request.Description,
                TransactionDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new AccountResponse
            {
                AccountId = account.ID,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance
            });
        }

        // ✅ Withdraw
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] TransactionRequest request)
        {
            if (request.Amount <= 0) return BadRequest("Amount must be greater than zero");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserID == userId);

            if (account == null)
                return NotFound("Account not found.");

            if (account.Balance < request.Amount)
                return BadRequest("Insufficient funds.");

            account.Balance -= request.Amount;

            _context.Transactions.Add(new Transaction
            {
                AccountId = account.ID,
                Amount = request.Amount,
                Type = TransactionType.Withdrawl,
                Description = request.Description,
                TransactionDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new AccountResponse
            {
                AccountId = account.ID,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance
            });
        }

        // ✅ Transaction History
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var transactions = await _context.Transactions
                .Where(t => t.Account.UserID == userId)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new TransactionResponse
                {
                    TransactionId = t.Id,
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate,
                    Description = t.Description,
                    Type = t.Type
                })
                .ToListAsync();

            return Ok(transactions);
        }
    }
}
