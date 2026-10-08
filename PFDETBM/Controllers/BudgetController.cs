using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PFDETBM.Data;
using PFDETBM.Models;
using PFDETBM.ViewModels;

namespace PFDETBM.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BudgetController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == CurrentUserId)
                .OrderByDescending(b => b.Year).ThenByDescending(b => b.Month)
                .ToListAsync();

            var expenses = await _context.Expenses
                .Where(e => e.UserId == CurrentUserId)
                .ToListAsync();

            var rows = budgets.Select(b =>
            {
                var bStart = new DateTime(b.Year, b.Month, 1);
                var bEnd = bStart.AddMonths(1);
                var spent = expenses.Where(e => e.CategoryId == b.CategoryId && e.Date >= bStart && e.Date < bEnd).Sum(e => e.Amount);
                return new BudgetProgressItem
                {
                    CategoryName = b.Category?.Name ?? "Uncategorized",
                    Spent = spent,
                    BudgetAmount = b.Amount,
                    PercentUsed = b.Amount == 0 ? 0 : Math.Round((double)(spent / b.Amount) * 100, 1),
                    Color = "#2563eb"
                };
            }).ToList();

            ViewBag.Budgets = budgets;
            return View(rows);
        }

        public async Task<IActionResult> Create()
        {
            var model = new BudgetFormViewModel { CategoryOptions = await GetCategoryOptionsAsync() };
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BudgetFormViewModel model)
        {
            var duplicate = await _context.Budgets.AnyAsync(b =>
                b.UserId == CurrentUserId && b.CategoryId == model.CategoryId && b.Month == model.Month && b.Year == model.Year);

            if (duplicate)
            {
                ModelState.AddModelError(string.Empty, "A budget for this category and month already exists. Edit it instead.");
            }

            if (!ModelState.IsValid)
            {
                model.CategoryOptions = await GetCategoryOptionsAsync();
                return View("Form", model);
            }

            var budget = new Budget
            {
                UserId = CurrentUserId,
                CategoryId = model.CategoryId,
                Amount = model.Amount,
                Month = model.Month,
                Year = model.Year
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Budget created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == CurrentUserId);
            if (budget == null)
            {
                return NotFound();
            }

            var model = new BudgetFormViewModel
            {
                Id = budget.Id,
                CategoryId = budget.CategoryId,
                Amount = budget.Amount,
                Month = budget.Month,
                Year = budget.Year,
                CategoryOptions = await GetCategoryOptionsAsync()
            };

            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BudgetFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var duplicate = await _context.Budgets.AnyAsync(b =>
                b.Id != id && b.UserId == CurrentUserId && b.CategoryId == model.CategoryId && b.Month == model.Month && b.Year == model.Year);

            if (duplicate)
            {
                ModelState.AddModelError(string.Empty, "A budget for this category and month already exists.");
            }

            if (!ModelState.IsValid)
            {
                model.CategoryOptions = await GetCategoryOptionsAsync();
                return View("Form", model);
            }

            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == CurrentUserId);
            if (budget == null)
            {
                return NotFound();
            }

            budget.CategoryId = model.CategoryId;
            budget.Amount = model.Amount;
            budget.Month = model.Month;
            budget.Year = model.Year;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Budget updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == CurrentUserId);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Budget deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetCategoryOptionsAsync()
        {
            return await _context.Categories
                .Where(c => c.IsExpense)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
        }
    }
}
