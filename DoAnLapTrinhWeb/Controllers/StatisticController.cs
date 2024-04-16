using DoAnLapTrinhWeb.Areas.Identity.Data;
using DoAnLapTrinhWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnLapTrinhWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
namespace DoAnLapTrinhWeb.Controllers
{
    public class StatisticController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;
        public StatisticController(ApplicationDbContext context, UserManager<AppliactionUser> userManager)
        {
            this._userManager = userManager;
            _context = context;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var incomeTransactions = await _context.Transactions
                .Where(x => x.UserID == userId && x.Category.Type == "Income")
                .Include(t => t.Category)
                .ToListAsync();

            var expenseTransactions = await _context.Transactions
                .Where(x => x.UserID == userId && x.Category.Type == "Expense")
                .Include(t => t.Category)
                .ToListAsync();

            var viewModel = new TransactionViewModel
            {
                IncomeTransactions = incomeTransactions,
                ExpenseTransactions = expenseTransactions
            };
            var incomeStatistics = new
            {
                HighestIncome = incomeTransactions.Any() ? incomeTransactions.Max(t => t.Amount) : 0,
                HighestIncomeName = incomeTransactions.Any() ? incomeTransactions.OrderByDescending(t => t.Amount).FirstOrDefault().CategoryNameWithIcon : "",
                DayWithHighestIncome = incomeTransactions.Any() ? incomeTransactions.OrderByDescending(t => t.Amount).FirstOrDefault().Date : DateTime.MinValue,
                MostFrequentIncomeCategory = incomeTransactions.GroupBy(t => t.CategoryNameWithIcon)
                                                                .OrderByDescending(g => g.Count())
                                                                .Select(g => g.Key)
                                                                .FirstOrDefault(),
                LargestIncomeTransactions = incomeTransactions
                .GroupBy(t => t.CategoryNameWithIcon) // Group transactions by category
                .SelectMany(group => group.OrderByDescending(t => t.Amount).Take(1)) // Select the highest transaction from each group
                .OrderByDescending(t => t.Amount) // Order the selected transactions by amount
                .Take(5), // Take the top 5 transactions
            };

            ViewBag.IncomeStatistics = incomeStatistics;

            var expenseStatistics = new
            {
                HighestExpense = expenseTransactions.Any() ? expenseTransactions.Max(t => t.Amount) : 0,
                HighestExpenseName = expenseTransactions.Any() ? expenseTransactions.OrderByDescending(t => t.Amount).FirstOrDefault().CategoryNameWithIcon : "",
                DayWithHighestExpense = expenseTransactions.Any() ? expenseTransactions.OrderByDescending(t => t.Amount).FirstOrDefault().Date : DateTime.MinValue,
                MostFrequentExpenseCategory = expenseTransactions.GroupBy(t => t.CategoryNameWithIcon)
                                                               .OrderByDescending(g => g.Count())
                                                               .Select(g => g.Key)
                                                               .FirstOrDefault(),
                largestExpenseTransactions = expenseTransactions
                .GroupBy(t => t.CategoryNameWithIcon) // Group transactions by category
                .SelectMany(group => group.OrderByDescending(t => t.Amount).Take(1)) // Select the highest transaction from each group
                .OrderByDescending(t => t.Amount) // Order the selected transactions by amount
                .Take(5), // Take the top 5 transactions
        };
            ViewBag.ExpenseStatistics = expenseStatistics;
            return View(viewModel);
        }
    }
}
