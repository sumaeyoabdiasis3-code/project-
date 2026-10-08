using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PFDETBM.Data;
using PFDETBM.ViewModels;

namespace PFDETBM.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private static readonly string[] Palette =
        {
            "#2563eb", "#16a34a", "#f59e0b", "#dc2626", "#7c3aed", "#0891b2", "#db2777", "#64748b"
        };

        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        public async Task<IActionResult> Index(int? month, int? year)
        {
            var now = DateTime.Now;
            var selectedMonth = month is >= 1 and <= 12 ? month.Value : now.Month;
            var selectedYear = year is >= 2000 and <= 2100 ? year.Value : now.Year;

            var monthStart = new DateTime(selectedYear, selectedMonth, 1);
            var monthEnd = monthStart.AddMonths(1);

            var incomes = await _context.Incomes
                .Where(i => i.UserId == CurrentUserId && i.Date >= monthStart && i.Date < monthEnd)
                .ToListAsync();

            var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == CurrentUserId && e.Date >= monthStart && e.Date < monthEnd)
                .ToListAsync();

            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == CurrentUserId && b.Month == selectedMonth && b.Year == selectedYear)
                .ToListAsync();

            var savingsGoals = await _context.SavingsGoals
                .Where(g => g.UserId == CurrentUserId)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();

            var incomeGroups = incomes
                .GroupBy(i => i.Source)
                .Select(g => new { Name = g.Key, Amount = g.Sum(i => i.Amount) })
                .OrderByDescending(g => g.Amount)
                .ToList();
            var incomeTotal = incomeGroups.Sum(g => g.Amount);

            var expenseGroups = expenses
                .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                .Select(g => new { Name = g.Key, Amount = g.Sum(e => e.Amount) })
                .OrderByDescending(g => g.Amount)
                .ToList();
            var expenseTotal = expenseGroups.Sum(g => g.Amount);

            var model = new ReportsViewModel
            {
                MonthLabel = monthStart.ToString("MMMM yyyy"),
                Month = selectedMonth,
                Year = selectedYear,
                TotalIncome = incomes.Sum(i => i.Amount),
                TotalExpenses = expenses.Sum(e => e.Amount),
                NetSavings = incomes.Sum(i => i.Amount) - expenses.Sum(e => e.Amount),
                IncomeBySource = incomeGroups.Select((g, index) => new CategorySlice
                {
                    Name = g.Name,
                    Amount = g.Amount,
                    Percent = incomeTotal == 0 ? 0 : Math.Round((double)(g.Amount / incomeTotal) * 100, 1),
                    Color = Palette[index % Palette.Length]
                }).ToList(),
                ExpenseByCategory = expenseGroups.Select((g, index) => new CategorySlice
                {
                    Name = g.Name,
                    Amount = g.Amount,
                    Percent = expenseTotal == 0 ? 0 : Math.Round((double)(g.Amount / expenseTotal) * 100, 1),
                    Color = Palette[index % Palette.Length]
                }).ToList(),
                BudgetPerformance = budgets.Select((b, index) =>
                {
                    var spent = expenses.Where(e => e.CategoryId == b.CategoryId).Sum(e => e.Amount);
                    return new BudgetProgressItem
                    {
                        CategoryName = b.Category?.Name ?? "Uncategorized",
                        Spent = spent,
                        BudgetAmount = b.Amount,
                        PercentUsed = b.Amount == 0 ? 0 : Math.Round((double)(spent / b.Amount) * 100, 1),
                        Color = Palette[index % Palette.Length]
                    };
                }).ToList(),
                SavingsGoals = savingsGoals
            };

            return View(model);
        }
    }
}
