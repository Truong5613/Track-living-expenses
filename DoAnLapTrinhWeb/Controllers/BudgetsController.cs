using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAnLapTrinhWeb.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using DoAnLapTrinhWeb.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace DoAnLapTrinhWeb.Controllers
{
    public class BudgetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;
        public BudgetsController(ApplicationDbContext context, UserManager<AppliactionUser> userManager)
        {
            _context = context;
            this._userManager = userManager;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Budgets.Where(x => x.UserId == _userManager.GetUserId(User)).Include(b => b.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        [Authorize]
        public IActionResult AddorEdit(int id = 0)
        {
            PopulateIncome();
            PopulateExpense();
            Budget budget;

            if (id == 0)
            {
                budget = new Budget
                {
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(7)
                };
            }
            else
            {
                budget = _context.Budgets.Find(id);
            }

            return View(budget);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddorEdit([Bind("BudgetId,Amount,StartDate,EndDate,CategoryId")] Budget budget)
        {
            if (ModelState.IsValid)
            {
                if (budget.BudgetId == 0)
                {
                    budget.UserId = _userManager.GetUserId(User);
                    _context.Budgets.Add(budget);
                    TempData["message"] = "Thêm budget thành công !";
                }
                else
                {
                    budget.UserId = _userManager.GetUserId(User);
                    _context.Budgets.Update(budget);
                    TempData["message"] = "Chỉnh budget thành công !";
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
            return View(budget);
           
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.BudgetId == id);
            if (budget == null)
            {
                return NotFound();
            }

            return View(budget);
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);

            if (budget != null)
            {
                _context.Budgets.Remove(budget);
            }

            await _context.SaveChangesAsync();
            TempData["message"] = "Xóa budget thành công !";
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
