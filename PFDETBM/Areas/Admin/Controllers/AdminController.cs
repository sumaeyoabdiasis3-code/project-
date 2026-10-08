using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PFDETBM.Data;
using PFDETBM.Models;
using PFDETBM.ViewModels;

namespace PFDETBM.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            var model = new AdminStatsViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalAdmins = admins.Count,
                TotalCategories = await _context.Categories.CountAsync(),
                TotalIncomeAllUsers = await _context.Incomes.SumAsync(i => (decimal?)i.Amount) ?? 0,
                TotalExpensesAllUsers = await _context.Expenses.SumAsync(e => (decimal?)e.Amount) ?? 0,
                TotalIncomeEntries = await _context.Incomes.CountAsync(),
                TotalExpenseEntries = await _context.Expenses.CountAsync(),
                TotalBudgets = await _context.Budgets.CountAsync(),
                TotalSavingsGoals = await _context.SavingsGoals.CountAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.OrderBy(u => u.Email).ToListAsync();
            var rows = new List<AdminUserRow>();

            foreach (var user in users)
            {
                rows.Add(new AdminUserRow
                {
                    Id = user.Id,
                    FullName = user.FullName ?? "(no name set)",
                    Email = user.Email ?? string.Empty,
                    IsAdmin = await _userManager.IsInRoleAsync(user, "Admin")
                });
            }

            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRole(string id)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (id == currentUserId)
            {
                TempData["Error"] = "You cannot change your own admin role.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "User");
                TempData["Success"] = $"{user.Email} is now a regular user.";
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                TempData["Success"] = $"{user.Email} is now an admin.";
            }

            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(categories);
        }

        public IActionResult CategoryCreate()
        {
            return View("CategoryForm", new CategoryFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryCreate(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("CategoryForm", model);
            }

            _context.Categories.Add(new Category { Name = model.Name, IsExpense = model.IsExpense });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully.";
            return RedirectToAction(nameof(Categories));
        }

        public async Task<IActionResult> CategoryEdit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View("CategoryForm", new CategoryFormViewModel { Id = category.Id, Name = category.Name, IsExpense = category.IsExpense });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryEdit(int id, CategoryFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View("CategoryForm", model);
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = model.Name;
            category.IsExpense = model.IsExpense;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category updated successfully.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var inUse = await _context.Expenses.AnyAsync(e => e.CategoryId == id) || await _context.Budgets.AnyAsync(b => b.CategoryId == id);
            if (inUse)
            {
                TempData["Error"] = "This category is used by existing expenses or budgets and cannot be deleted.";
                return RedirectToAction(nameof(Categories));
            }

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category deleted.";
            }

            return RedirectToAction(nameof(Categories));
        }
    }
}
