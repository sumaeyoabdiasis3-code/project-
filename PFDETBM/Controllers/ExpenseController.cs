using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PFDETBM.Data;
using PFDETBM.Helpers;
using PFDETBM.Models;
using PFDETBM.ViewModels;

namespace PFDETBM.Controllers
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Expenses.Include(e => e.Category).Where(e => e.UserId == CurrentUserId);
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    (e.Description != null && e.Description.Contains(search)) ||
                    (e.Category != null && e.Category.Name.Contains(search)));
            }

            var expenses = await query.OrderByDescending(e => e.Date).ToListAsync();
            ViewData["Search"] = search;
            ViewData["Total"] = expenses.Sum(e => e.Amount);
            return View(expenses);
        }

        public async Task<IActionResult> Create()
        {
            var model = new ExpenseFormViewModel { CategoryOptions = await GetCategoryOptionsAsync() };
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CategoryOptions = await GetCategoryOptionsAsync();
                return View("Form", model);
            }

            var expense = new Expense
            {
                UserId = CurrentUserId,
                CategoryId = model.CategoryId,
                Amount = model.Amount,
                Date = model.Date,
                PaymentMethod = model.PaymentMethod,
                Description = model.Description
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            await BudgetAlertGenerator.CheckAndNotifyAsync(_context, CurrentUserId, model.CategoryId, model.Date.Month, model.Date.Year);

            TempData["Success"] = "Expense added successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == CurrentUserId);
            if (expense == null)
            {
                return NotFound();
            }

            var model = new ExpenseFormViewModel
            {
                Id = expense.Id,
                CategoryId = expense.CategoryId,
                Amount = expense.Amount,
                Date = expense.Date,
                PaymentMethod = expense.PaymentMethod,
                Description = expense.Description,
                CategoryOptions = await GetCategoryOptionsAsync()
            };

            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                model.CategoryOptions = await GetCategoryOptionsAsync();
                return View("Form", model);
            }

            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == CurrentUserId);
            if (expense == null)
            {
                return NotFound();
            }

            expense.CategoryId = model.CategoryId;
            expense.Amount = model.Amount;
            expense.Date = model.Date;
            expense.PaymentMethod = model.PaymentMethod;
            expense.Description = model.Description;

            await _context.SaveChangesAsync();

            await BudgetAlertGenerator.CheckAndNotifyAsync(_context, CurrentUserId, model.CategoryId, model.Date.Month, model.Date.Year);

            TempData["Success"] = "Expense updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == CurrentUserId);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Expense deleted.";
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
