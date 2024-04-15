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
        [Authorize]
        public async Task<ActionResult> Index(int numberOfDays = 7)
        {
            ViewData["UserID"] = _userManager.GetUserId(this.User);
            await GenerateRecurringTransactions();
            //7days
            var userCategoriesCount = await _context.Categories
        .Where(c => c.UserID == _userManager.GetUserId(User))
        .CountAsync();

            if (userCategoriesCount < 10)
            {
                // Add additional categories dynamically
                await AddAdditionalCategories(_userManager.GetUserId(User));
            }
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

            Transaction transaction = new Transaction();



            return View(transaction);
        }
        public class SplineChartData()
        {
            public string day;
            public int income;
            public int expense;
        }
        public async Task GenerateRecurringTransactions()
        {
            var recurringTransactions = await _context.RecurringTransactions.ToListAsync();
            var successMessages = new List<string>();

            foreach (var recurringTransaction in recurringTransactions)
            {
                DateTime lastProcessedDate = recurringTransaction.LastProcessedDate ?? recurringTransaction.StartDate;

                while (IsTimeToGenerateTransaction(recurringTransaction, lastProcessedDate))
                {
                    // Generate a new transaction based on the recurring transaction details
                    var newTransaction = new Transaction
                    {
                        Amount = recurringTransaction.Amount,
                        Note = recurringTransaction.Note,
                        CategoryId = recurringTransaction.CategoryId,
                        UserID = recurringTransaction.UserID,
                        Date = lastProcessedDate.Date
                    };
                    _context.Add(newTransaction);
                    await _context.SaveChangesAsync();
                    UpdateNextOccurrence(recurringTransaction);
                    var category = await _context.Categories.FindAsync(recurringTransaction.CategoryId);
                    successMessages.Add($"Thêm {recurringTransaction.Amount:C0} vào {category.Name} thành công!");

                    switch (recurringTransaction.RecurrenceInterval.ToLower())
                    {
                        case "hằng ngày":
                            lastProcessedDate = lastProcessedDate.AddDays(1);
                            break;
                        case "hằng tuần":
                            lastProcessedDate = lastProcessedDate.AddDays(7);
                            break;
                        case "hằng tháng":
                            lastProcessedDate = lastProcessedDate.AddMonths(1);
                            break;
                        case "hằng năm":
                            lastProcessedDate = lastProcessedDate.AddYears(1);
                            break;
                        default:
                            throw new NotImplementedException($"Recurrence interval '{recurringTransaction.RecurrenceInterval}' is not implemented.");
                    }
                }
                recurringTransaction.LastProcessedDate = lastProcessedDate; // Update the last processed date
            }
            // Add the list of messages to TempData
            TempData["successMessages"] = successMessages;
        }
        private async Task AddAdditionalCategories(string userId)
        {
            // Define the additional categories you want to add
            var additionalCategories = new List<Category>
    {
        new Category { Name = "Ăn Uống", Icon = "🍴", Type = "Expense", UserID = userId },
        new Category { Name = "Đi chợ", Icon = "🛒", Type = "Expense", UserID = userId },
        new Category { Name = "Phương tiện công cộng", Icon = "🚃", Type = "Expense", UserID = userId },
        new Category { Name = "Giải Trí", Icon = "🍿", Type = "Expense", UserID = userId },
        new Category { Name = "Trả Phí", Icon = "🧾", Type = "Expense", UserID = userId },
        new Category { Name = "Quà Tặng", Icon = "🎁", Type = "Expense", UserID = userId },
        new Category { Name = "Làm Đẹp", Icon = "💄", Type = "Expense", UserID = userId },
        new Category { Name = "Đi làm", Icon = "💸", Type = "Expense", UserID = userId },
        new Category { Name = "Du Lịch", Icon = "✈️", Type = "Expense", UserID = userId },
        new Category { Name = "Lương", Icon = "💰", Type = "Income", UserID = userId },
    };

            // Add the additional categories to the database
            _context.Categories.AddRange(additionalCategories);
            await _context.SaveChangesAsync();
        }

        private void UpdateNextOccurrence(RecurringTransaction recurringTransaction)
{
    switch (recurringTransaction.RecurrenceInterval.ToLower())
    {
        case "hằng ngày":
            recurringTransaction.StartDate = recurringTransaction.StartDate.AddDays(1);
            break;
        case "hằng tuần":
            recurringTransaction.StartDate = recurringTransaction.StartDate.AddDays(7);
            break;
        case "hàng tháng":
            recurringTransaction.StartDate = recurringTransaction.StartDate.AddMonths(1);
            break;
        case "hằng năm":
            recurringTransaction.StartDate = recurringTransaction.StartDate.AddYears(1);
            break;
    }

    // Update the database with the new start date and end date
    _context.Update(recurringTransaction);
    _context.SaveChanges();
}

        private bool IsTimeToGenerateTransaction(RecurringTransaction recurringTransaction, DateTime currentDate)
        {
            switch (recurringTransaction.RecurrenceInterval.ToLower())
            {
                case "hằng ngày":
                    return currentDate <= DateTime.Today;
                case "hằng tuần":
                    return currentDate <= DateTime.Today;
                case "hằng tháng":
                    return currentDate.Day == recurringTransaction.StartDate.Day && currentDate <= DateTime.Today;
                case "hằng năm":
                    return currentDate.Day == recurringTransaction.StartDate.Day && currentDate.Month == recurringTransaction.StartDate.Month && currentDate <= DateTime.Today;
                default:
                    return false;
            }
        }

    }
}
