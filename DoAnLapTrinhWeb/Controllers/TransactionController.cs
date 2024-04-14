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
using Microsoft.AspNetCore.Authorization;

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
                .Where(t => t.Date.Year == year && t.UserID == _userManager.GetUserId(User));

            // If month is not 0, filter transactions by month as well
            if (month != 0)
            {
                transactions = transactions.Where(t => t.Date.Month == month);
            }

            var transactionList = transactions
                .Include(t => t.Category)
                .ToList();

            byte[] fileContents;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Transactions");

                // Add the header with the appropriate month/year text
                string headerText = month == 0 ? $"Giao dịch {year}" : $"Giao dịch {month}/{year}";
                worksheet.Cells["A1"].Value = headerText;
                worksheet.Cells["A1:C1"].Merge = true; // Merge cells for the header
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Headers for income section
                worksheet.Cells[3, 1].Value = "Thu nhập";
                worksheet.Cells[4, 1].Value = "Giao Dịch";
                worksheet.Cells[4, 2].Value = "Ngày";
                worksheet.Cells[4, 3].Value = "Số Tiền";

                // Apply styling to income section header row
                using (var range = worksheet.Cells[3, 1, 3, 3])
                {
                    range.Merge = true;
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Data for income table
                int rowIncome = 5;
                decimal totalIncome = 0;

                foreach (var transaction in transactionList.Where(t => t.Category.Type == "Income"))
                {
                    worksheet.Cells[rowIncome, 1].Value = transaction.CategoryNameWithIcon;
                    worksheet.Cells[rowIncome, 2].Value = transaction.Date.ToString("MM-dd-yyyy");
                    worksheet.Cells[rowIncome, 3].Value = transaction.Amount;
                    worksheet.Cells[rowIncome, 3].Style.Font.Color.SetColor(System.Drawing.Color.Green); // Set font color to green for income
                    totalIncome += transaction.Amount;
                    rowIncome++;
                }

                // Headers for expense section
                int rowExpenseHeader = rowIncome + 2;
                worksheet.Cells[rowExpenseHeader, 1].Value = "Tiêu dùng";
                worksheet.Cells[rowExpenseHeader + 1, 1].Value = "Giao Dịch";
                worksheet.Cells[rowExpenseHeader + 1, 2].Value = "Ngày";
                worksheet.Cells[rowExpenseHeader + 1, 3].Value = "Số Tiền";

                // Apply styling to expense section header row
                using (var range = worksheet.Cells[rowExpenseHeader, 1, rowExpenseHeader, 3])
                {
                    range.Merge = true;
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Data for expense table
                int rowExpense = rowExpenseHeader + 2;
                decimal totalExpense = 0;

                foreach (var transaction in transactionList.Where(t => t.Category.Type == "Expense"))
                {
                    worksheet.Cells[rowExpense, 1].Value = transaction.CategoryNameWithIcon;
                    worksheet.Cells[rowExpense, 2].Value = transaction.Date.ToString("MM-dd-yyyy");
                    worksheet.Cells[rowExpense, 3].Value = -transaction.Amount; // Make it negative for expenses
                    worksheet.Cells[rowExpense, 3].Style.Font.Color.SetColor(System.Drawing.Color.Red); // Set font color to red for expenses
                    totalExpense += transaction.Amount;
                    rowExpense++;
                }

                // Total table
                int rowTotalHeader = rowExpense + 2;
                worksheet.Cells[rowTotalHeader, 1].Value = "Tổng tiền";
                worksheet.Cells[rowTotalHeader, 2].Value = "";
                worksheet.Cells[rowTotalHeader, 3].Value = "";

                // Apply styling to total header row
                using (var range = worksheet.Cells[rowTotalHeader, 1, rowTotalHeader, 3])
                {
                    range.Merge = true;
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Data for total table
                int rowTotal = rowTotalHeader + 1;
                worksheet.Cells[rowTotal, 1].Value = "Tổng Thu";
                worksheet.Cells[rowTotal, 2].Value = "";
                worksheet.Cells[rowTotal, 3].Value = totalIncome;
                worksheet.Cells[rowTotal, 3].Style.Font.Color.SetColor(System.Drawing.Color.Green); // Set font color to green for total income

                worksheet.Cells[rowTotal + 1, 1].Value = "Tổng Chi";
                worksheet.Cells[rowTotal + 1, 2].Value = "";
                worksheet.Cells[rowTotal + 1, 3].Value = totalExpense;
                worksheet.Cells[rowTotal + 1, 3].Style.Font.Color.SetColor(System.Drawing.Color.Red); // Set font color to red for total expense

                worksheet.Cells[rowTotal + 2, 1].Value = "Tổng";
                worksheet.Cells[rowTotal + 2, 2].Value = "";
                worksheet.Cells[rowTotal + 2, 3].Formula = $"C{rowTotal} - C{rowTotal + 1}"; // Calculate balance
                worksheet.Cells[rowTotal + 2, 3].Style.Font.Color.SetColor(totalIncome - totalExpense >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red); // Set font color to green for positive and red for negative

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Set borders for all cells
                worksheet.Cells[3, 1, rowTotal + 2, 3].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[3, 1, rowTotal + 2, 3].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[3, 1, rowTotal + 2, 3].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[3, 1, rowTotal + 2, 3].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                fileContents = package.GetAsByteArray();
            }

            // Generate file name
            string fileName;
            if (month == 0)
            {
                fileName = $"Transactions_{year}.xlsx";
            }
            else
            {
                fileName = $"Transactions_{month}-{year}.xlsx";
            }

            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        // GET: Transaction
        [Authorize]
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
        [Authorize]
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

            return RedirectToAction("Index", "DashBoard", transaction);
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
