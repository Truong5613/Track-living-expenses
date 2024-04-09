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

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Budgets.Where(x => x.UserId == _userManager.GetUserId(User)).Include(b => b.Category);
            return View(await applicationDbContext.ToListAsync());
        }


        public IActionResult AddorEdit(int id=0)
        {
            PopulateCategories();
            if (id == 0)
                return View(new Budget());
            else
                return View(_context.Budgets.Find(id));
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
                }
                else
                {
                    budget.UserId = _userManager.GetUserId(User);
                    _context.Budgets.Update(budget);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in ModelState.Values)
            {
                Console.WriteLine(error.Errors.FirstOrDefault()?.ErrorMessage);
            }
            PopulateCategories();
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
            return RedirectToAction(nameof(Index));
        }

        private bool BudgetExists(int id)
        {
            return _context.Budgets.Any(e => e.BudgetId == id);
        }
        [NonAction]
        public void PopulateCategories()
        {
            var categoryCollection = _context.Categories.Where(x => x.UserID == _userManager.GetUserId(User)).ToList();
            Category defaultCategory = new Category() { CategoryId = 0, Name = "Chọn một danh mục" };
            categoryCollection.Insert(0, defaultCategory);
            ViewBag.Categories = categoryCollection;
        }
    }
}
