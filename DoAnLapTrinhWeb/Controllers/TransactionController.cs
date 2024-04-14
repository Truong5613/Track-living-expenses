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
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using System.IO;
using Microsoft.Extensions.Hosting.Internal;
using System.Globalization;
using System.ComponentModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DoAnLapTrinhWeb.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppliactionUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public TransactionController(ApplicationDbContext context, UserManager<AppliactionUser> userManager, IWebHostEnvironment hostingEnvironment)
        {
            this._userManager = userManager;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        public IActionResult ExportToExcel(int month, int year)
        {
            // Filter transactions based on the selected month and year
            var transactions = _context.Transactions
                .Where(t => t.Date.Month == month && t.Date.Year == year && t.UserID == _userManager.GetUserId(User))
                .Include(t => t.Category)
                .ToList();

            byte[] fileContents;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Transactions");

                // Headers
                worksheet.Cells[1, 1].Value = "Giao Dịch";
                worksheet.Cells[1, 2].Value = "Ngày";
                worksheet.Cells[1, 3].Value = "Số Tiền";

                // Apply styling to header row
                using (var range = worksheet.Cells[1, 1, 1, 3])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Data
                int row = 2;
                decimal totalIncome = 0;
                decimal totalExpense = 0;

                foreach (var transaction in transactions)
                {
                    worksheet.Cells[row, 1].Value = transaction.CategoryNameWithIcon;
                    worksheet.Cells[row, 2].Value = transaction.Date.ToString("MM-dd-yyyy");

                    // Set the amount based on income or expense and apply formatting
                    if (transaction.Category.Type == "Income")
                    {
                        worksheet.Cells[row, 3].Value = transaction.Amount;
                        worksheet.Cells[row, 3].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                        totalIncome += transaction.Amount;
                    }
                    else if (transaction.Category.Type == "Expense")
                    {
                        worksheet.Cells[row, 3].Value = -transaction.Amount; // Make it negative for expenses
                        worksheet.Cells[row, 3].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                        totalExpense += transaction.Amount;
                    }
                    else
                    {
                        worksheet.Cells[row, 3].Value = transaction.Amount; // Default behavior
                    }

                    row++;
                }

                // Tính Tổng thu nhập
                worksheet.Cells[row, 1].Value = "Tổng Thu";
                worksheet.Cells[row, 2].Value = "";
                worksheet.Cells[row, 3].Value = totalIncome;
                worksheet.Cells[row, 3].Style.Font.Color.SetColor(System.Drawing.Color.Green); // Đặt font color màu xanh 
                //Tính Tổng Chi 
                worksheet.Cells[row + 1, 1].Value = "Tổng Chi";
                worksheet.Cells[row + 1, 2].Value = "";
                worksheet.Cells[row + 1, 3].Value = totalExpense;
                worksheet.Cells[row + 1, 3].Style.Font.Color.SetColor(System.Drawing.Color.Red); // Đặt font color màu đỏ

                worksheet.Cells[row + 2, 1].Value = "Tổng Cân Đối";
                worksheet.Cells[row + 2, 2].Value = "";
                worksheet.Cells[row + 2, 3].Formula = $"C{row} - C{row + 1}"; // Tính tổng

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Set borders for all cells
                worksheet.Cells[1, 1, row + 2, 3].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1, row + 2, 3].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1, row + 2, 3].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1, row + 2, 3].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                fileContents = package.GetAsByteArray();
            }

            var fileName = $"Transactions_{month}-{year}.xlsx";

            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        // GET: Transaction
        public async Task<IActionResult> Index(int month = 0, int year =0)
        {
            
            var applicationDbContext = _context.Transactions.Where(x => x.UserID == _userManager.GetUserId(User)).Include(t => t.Category);
            if (month != 0 || year != 0)
            {
                ViewBag.month = month; ViewBag.year = year;
                var query = _context.Transactions.Where(x => x.UserID == _userManager.GetUserId(User));
                if (month == 0)
                {
                    query = query.Where(x => x.Date.Year == year);
                    var transactions = query.Include(t => t.Category).ToList();
                    return View(transactions);
                }
                else
                {
                    query = query.Where(x => x.Date.Year == year && x.Date.Month == month)
                        ; var transactions = query.Include(t => t.Category).ToList();
                    return View(transactions);

                }
            }

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
                    TempData["message"] = "Thêm giao dịch thành công";
                }
                else
                {
                    transaction.UserID = _userManager.GetUserId(User);
                    _context.Update(transaction);
                    TempData["message"] = "Chỉnh giao dịch thành công";
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddorEditBud([Bind("TransactionId,Amount,Note,Date,CategoryId")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                if (transaction.TransactionId == 0)
                {
                    transaction.UserID = _userManager.GetUserId(User);
                    _context.Add(transaction);
                    TempData["message"] = "Thêm giao dịch thành công";
                }
                else
                {
                    transaction.UserID = _userManager.GetUserId(User);
                    _context.Update(transaction);
                    TempData["message"] = "Chỉnh giao dịch thành công";
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index","DashBoard");
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



        public IActionResult AddorEditBudget(int id = 0, int CategoryId = 0 )
        {
            PopulateCategories();
            PopulateIncome();
            PopulateExpense();
            ViewBag.Cate = _context.Categories.Find(CategoryId);
            if (id == 0)
                return View(new Transaction());
            else
                return View(_context.Transactions.Find(id));
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
            TempData["message"] = "Xóa giao dịch thành công";
            return RedirectToAction(nameof(Index));
        }
        


        [NonAction]
        public void PopulateCategories()
        {
            var categoryCollection = _context.Categories.Where(x => x.UserID == _userManager.GetUserId(User)).ToList();
            Category defaultCategory = new Category() { CategoryId = 0, Name = "Chọn một danh mục" } ;
               categoryCollection.Insert(0, defaultCategory);
            ViewBag.Categories = categoryCollection;
        }
        public void PopulateIncome()
        {
            var categoryCollection = _context.Categories.Where(x => x.UserID == _userManager.GetUserId(User) && x.Type=="Income").ToList();
            Category defaultCategory = new Category() { CategoryId = 0, Name = "Chọn một danh mục" };
            categoryCollection.Insert(0, defaultCategory);
            ViewBag.IncomeCategories = categoryCollection;
        }
        public void PopulateExpense()
        {
            var categoryCollection = _context.Categories.Where(x => x.UserID == _userManager.GetUserId(User) && x.Type == "Expense").ToList();
            Category defaultCategory = new Category() { CategoryId = 0, Name = "Chọn một danh mục" };
            categoryCollection.Insert(0, defaultCategory);
            ViewBag.ExpenseCategories = categoryCollection;
        }
    }
}
