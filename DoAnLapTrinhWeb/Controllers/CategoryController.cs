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
using Microsoft.AspNetCore.Authorization;

namespace DoAnLapTrinhWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;
        public CategoryController(ApplicationDbContext context, UserManager<AppliactionUser> userManager)
        {
            this._userManager = userManager;
            _context = context;
        }

        // GET: Category
        [Authorize]
        public async Task<IActionResult> Index()
        {
            ViewData["UserID"] = _userManager.GetUserId(this.User);
            return View(await _context.Categories.Where(x => x.UserID == _userManager.GetUserId(User)).ToListAsync());
        }


        // GET: Category/Create
        [Authorize]
        public IActionResult AddorEdit(int id =0)
        {
            if(id==0)
                return View(new Category());
            else
                return View(_context.Categories.Find(id));
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddorEdit([Bind("CategoryId,Name,Icon,Type")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.CategoryId == 0)
                {
                    category.UserID = _userManager.GetUserId(User);
                    _context.Add(category);
                    TempData["message"] = "Thêm danh mục thành công";
                }
                else
                {
                    category.UserID = _userManager.GetUserId(User);
                    _context.Update(category);
                    TempData["message"] = "Chỉnh danh mục thành công";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }


        // GET: Category/Delete/5

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Thực thể 'ApplicationDbContest.Categories' đang trống");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            TempData["message"] = "Xóa danh mục thành công";
            return RedirectToAction(nameof(Index));
        }

    }
}
