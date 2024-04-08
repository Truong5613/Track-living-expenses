using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAnLapTrinhWeb.Models;
using DoAnLapTrinhWeb.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace DoAnLapTrinhWeb.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;
        public TransactionController(ApplicationDbContext context, UserManager<AppliactionUser> userManager)
        {
            this._userManager = userManager;
            _context = context;
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Transactions.Where(x => x.UserID == _userManager.GetUserId(User)).Include(t => t.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Transaction/Create
        public IActionResult AddorEdit(int id=0)
        {
            PopulateCategories();
            PopulateIncome();
            PopulateExpense();
            if (id == 0)
                return View(new Transaction());
            else
                return View(_context.Transactions.Find(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddorEdit([Bind("TransactionId,Amount,Note,Date,CategoryId")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                if (transaction.TransactionId == 0)
                {
                    transaction.UserID = _userManager.GetUserId(User);
                    _context.Add(transaction);
                }
                else
                {
                    transaction.UserID = _userManager.GetUserId(User);
                    _context.Update(transaction);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in ModelState.Values)
            {
                Console.WriteLine(error.Errors.FirstOrDefault()?.ErrorMessage);
            }
            PopulateCategories();
            PopulateExpense();
            PopulateIncome();

            return View(transaction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Transactions == null)
            {
                return Problem("Thực thể 'ApplicationDbContest.Transactions' đang trống");
            }
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [NonAction]
        public void PopulateCategories()
        {
            var categoryCollection = _context.Categories.Where(x => x.UserID == _userManager.GetUserId(User)).ToList();
            Category defaultCategory = new Category() { CategoryId = 0, Name = "Chọn một Category" } ;
               categoryCollection.Insert(0, defaultCategory);
            ViewBag.Categories = categoryCollection;
        }
        public void PopulateIncome()
        {
            var categoryCollection = _context.Categories.Where(x => x.UserID == _userManager.GetUserId(User) && x.Type=="Income").ToList();
            Category defaultCategory = new Category() { CategoryId = 0, Name = "Chọn một Category" };
            categoryCollection.Insert(0, defaultCategory);
            ViewBag.IncomeCategories = categoryCollection;
        }
        public void PopulateExpense()
        {
            var categoryCollection = _context.Categories.Where(x => x.UserID == _userManager.GetUserId(User) && x.Type == "Expense").ToList();
            Category defaultCategory = new Category() { CategoryId = 0, Name = "Chọn một Category" };
            categoryCollection.Insert(0, defaultCategory);
            ViewBag.ExpenseCategories = categoryCollection;
        }
    }
}
