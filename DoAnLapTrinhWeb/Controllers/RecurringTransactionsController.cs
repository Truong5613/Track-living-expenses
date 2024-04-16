using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAnLapTrinhWeb.Models;
using Microsoft.AspNetCore.Identity;
using DoAnLapTrinhWeb.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace DoAnLapTrinhWeb.Controllers
{
    public class RecurringTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;

        public RecurringTransactionsController(ApplicationDbContext context,UserManager<AppliactionUser> userManager)
        {
            _context = context;
            this._userManager = userManager;
        }

        // GET: RecurringTransactions
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.RecurringTransactions.Where(x => x.UserID == _userManager.GetUserId(User)).Include(b => b.Category);
            return View(await applicationDbContext.ToListAsync());
        }
        [Authorize]
        public IActionResult AddorEdit(int id = 0)
        {
            PopulateIncome();
            PopulateExpense();
            RecurringTransaction recurringTransaction;

            if (id == 0)
            {
                recurringTransaction = new RecurringTransaction
                {
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(7)
                };
            }
            else
            {
                recurringTransaction = _context.RecurringTransactions.Find(id);
            }
            return View(recurringTransaction);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddorEdit([Bind("RecurringTransactionId,Amount,Note,CategoryId,UserID,RecurrenceInterval,StartDate,EndDate")] RecurringTransaction recurringTransaction)
        {
            if (ModelState.IsValid)
            {
                if (recurringTransaction.RecurringTransactionId == 0)
                {
                    recurringTransaction.UserID = _userManager.GetUserId(User);
                    _context.RecurringTransactions.Add(recurringTransaction);
                    TempData["message"] = "Thêm Subscription thành công!";
                }
                else
                {
                    recurringTransaction.UserID = _userManager.GetUserId(User);
                    _context.RecurringTransactions.Update(recurringTransaction);
                    TempData["message"] = "Chỉnh Subscription thành công!";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in ModelState.Values)
            {
                Console.WriteLine(error.Errors.FirstOrDefault()?.ErrorMessage);
            }
            PopulateIncome();
            PopulateExpense();
            return View(recurringTransaction);

        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recurringTransaction = await _context.RecurringTransactions.FindAsync(id);
            if (recurringTransaction != null)
            {
                _context.RecurringTransactions.Remove(recurringTransaction);
            }

            await _context.SaveChangesAsync();
            TempData["message"] = "Xoá Subscription thành công!";
            return RedirectToAction(nameof(Index));
        }
        [NonAction]
        public void PopulateIncome()
        {
            var categoryCollection = _context.Categories.Where(x => x.UserID == _userManager.GetUserId(User) && x.Type == "Income").ToList();
            Category defaultCategory = new Category() { CategoryId = 0, Name = "Chọn một danh mục" };
            categoryCollection.Insert(0, defaultCategory);
            ViewBag.IncomeCategories = categoryCollection;
        }
        [NonAction]
        public void PopulateExpense()
        {
            var categoryCollection = _context.Categories.Where(x => x.UserID == _userManager.GetUserId(User) && x.Type == "Expense").ToList();
            Category defaultCategory = new Category() { CategoryId = 0, Name = "Chọn một danh mục" };
            categoryCollection.Insert(0, defaultCategory);
            ViewBag.ExpenseCategories = categoryCollection;
        }
    }
}
