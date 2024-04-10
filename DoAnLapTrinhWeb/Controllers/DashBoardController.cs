using System.Globalization;
using DoAnLapTrinhWeb.Areas.Identity.Data;
using DoAnLapTrinhWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb.Controllers
{
    [Authorize]
    public class DashBoardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;
        public DashBoardController(ApplicationDbContext context, UserManager<AppliactionUser> userManager)
        {
            this._userManager = userManager;
            _context = context;
        }

        public async Task<ActionResult> Index(int numberOfDays = 7)
        {
            ViewData["UserID"] = _userManager.GetUserId(this.User);
            //7days
            DateTime StartDate = DateTime.Today.AddDays(-numberOfDays + 1);
            DateTime Endate = DateTime.Today;
            @ViewBag.numberOfDays = numberOfDays;

            List<Transaction> SelectedTransaction = await _context.Transactions.Include(x=>x.Category)
              .Where(y=>y.Date >= StartDate && y.Date <=Endate && y.UserID == _userManager.GetUserId(User)).ToListAsync();

            // Total Income
            int TotalIncome = SelectedTransaction.Where(x => x.Category.Type == "Income" && x.UserID == _userManager.GetUserId(User)).Sum(y => y.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("C0");

            // Total Expense
            int TotalExpense = SelectedTransaction.Where(x => x.Category.Type == "Expense" && x.UserID == _userManager.GetUserId(User)).Sum(y => y.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C0");

            // Balance
            int Balance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = String.Format(culture,"{0:C0}",Balance);

            //Doughtnut Chart - Expense By Category
            ViewBag.ExpenseDoughnutChartData = SelectedTransaction.Where(x => x.Category.Type == "Expense" && x.UserID == _userManager.GetUserId(User))
                 .GroupBy(y => y.Category.CategoryId)
                 .Select(z => new
                 {
                     CategoryTitleWithIcon = z.First().Category.Icon + " " + z.First().Category.Name,
                     amount = z.Sum(n => n.Amount),
                     FormattedAmount = z.Sum(n => n.Amount).ToString("C0"),
                 }).OrderByDescending(m => m.amount).ToList();

            ViewBag.IncomeDoughnutChartData = SelectedTransaction.Where(x => x.Category.Type == "Income")
                .GroupBy(y => y.Category.CategoryId)
                .Select(z => new
                {
                    CategoryTitleWithIcon = z.First().Category.Icon + " " + z.First().Category.Name,
                    amount = z.Sum(n => n.Amount),
                    FormattedAmount = z.Sum(n => n.Amount).ToString("C0"),
                }).OrderByDescending(m => m.amount).ToList();

            //spline chart - Income vs Expense
            //income
            List<SplineChartData> IncomeSummary = SelectedTransaction.Where(x => x.Category.Type == "Income" && x.UserID == _userManager.GetUserId(User)).GroupBy(y => y.Date)
                .Select(z => new SplineChartData()
                {
                    day = z.First().Date.ToString("MM-dd"),
                    income = z.Sum(i=>i.Amount),
                }).ToList();

            //expense
            List<SplineChartData> ExpenseSummary = SelectedTransaction.Where(x => x.Category.Type == "Expense" && x.UserID == _userManager.GetUserId(User)).GroupBy(y => y.Date)
                .Select(z => new SplineChartData()
                {
                    day = z.First().Date.ToString("MM-dd"),
                    expense = z.Sum(i => i.Amount),
                }).ToList();

            //combine Income & Expense
            string[] last7days = Enumerable.Range(0,numberOfDays).Select(x=>StartDate.AddDays(x).ToString("MM-dd"))
                .ToArray();

            ViewBag.SplineChartData = from day in last7days
                                      join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };
            //Recent Transations 
            ViewBag.RecentTransactions = await _context.Transactions.Include(i => i.Category).OrderByDescending(x => x.Date).Take(5).Where( x => x.UserID == _userManager.GetUserId(User)).ToListAsync();

            ViewBag.TransactionsBudger = await _context.Transactions.Include(i => i.Category).OrderByDescending(x => x.Date).Where(x => x.UserID == _userManager.GetUserId(User)).ToListAsync();

            ViewBag.BudGetList = await _context.Budgets.Include(i => i.Category).OrderByDescending(x => x.StartDate).Where(x => x.UserId == _userManager.GetUserId(User)).ToListAsync();

           


            return View();
        }
        public class SplineChartData()
        {
            public string day;
            public int income;
            public int expense;
        }
    }
}
