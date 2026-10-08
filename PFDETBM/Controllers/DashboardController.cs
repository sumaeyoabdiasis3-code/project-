using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PFDETBM.Data;
using PFDETBM.Helpers;
using PFDETBM.ViewModels;

namespace PFDETBM.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private static readonly string[] Palette =
        {
            "#2563eb", "#16a34a", "#f59e0b", "#dc2626", "#7c3aed", "#0891b2", "#db2777", "#64748b"
        };

        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Challenge();
            }

            var incomes = await _context.Incomes
                .Where(i => i.UserId == userId)
                .ToListAsync();

            var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var recentTransactions = await TransactionFeedBuilder.BuildAsync(_context, userId, take: 10);

            var savingsGoals = await _context.SavingsGoals
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.TargetDate)
                .Take(3)
                .ToListAsync();

            var storedNotifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            var now = DateTime.Now;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);

            var currentMonthIncome = incomes
                .Where(i => i.Date >= currentMonthStart && i.Date < currentMonthStart.AddMonths(1))
                .Sum(i => i.Amount);
            var previousMonthIncome = incomes
                .Where(i => i.Date >= previousMonthStart && i.Date < currentMonthStart)
                .Sum(i => i.Amount);

            var currentMonthExpenses = expenses
                .Where(e => e.Date >= currentMonthStart && e.Date < currentMonthStart.AddMonths(1))
                .Sum(e => e.Amount);
            var previousMonthExpenses = expenses
                .Where(e => e.Date >= previousMonthStart && e.Date < currentMonthStart)
                .Sum(e => e.Amount);

            var allTimeIncome = incomes.Sum(i => i.Amount);
            var allTimeExpenses = expenses.Sum(e => e.Amount);
            var currentBalance = allTimeIncome - allTimeExpenses;
            var previousBalance = currentBalance - (currentMonthIncome - currentMonthExpenses);

            var currentMonthSavings = currentMonthIncome - currentMonthExpenses;
            var previousMonthSavings = previousMonthIncome - previousMonthExpenses;

            var monthlyBudget = budgets
                .Where(b => b.Month == now.Month && b.Year == now.Year)
                .Sum(b => b.Amount);
            var remainingBudget = monthlyBudget - currentMonthExpenses;

            var categoryGroups = expenses
                .Where(e => e.Date >= currentMonthStart && e.Date < currentMonthStart.AddMonths(1))
                .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                .Select(g => new { Name = g.Key, Amount = g.Sum(e => e.Amount) })
                .OrderByDescending(g => g.Amount)
                .ToList();

            var categoryTotal = categoryGroups.Sum(g => g.Amount);
            var expenseByCategory = categoryGroups
                .Select((g, index) => new CategorySlice
                {
                    Name = g.Name,
                    Amount = g.Amount,
                    Percent = categoryTotal == 0 ? 0 : Math.Round((double)(g.Amount / categoryTotal) * 100, 1),
                    Color = Palette[index % Palette.Length]
                })
                .ToList();

            var budgetProgress = budgets
                .Where(b => b.Month == now.Month && b.Year == now.Year)
                .Select((b, index) =>
                {
                    var spent = expenses
                        .Where(e => e.CategoryId == b.CategoryId
                            && e.Date >= currentMonthStart && e.Date < currentMonthStart.AddMonths(1))
                        .Sum(e => e.Amount);
                    return new BudgetProgressItem
                    {
                        CategoryName = b.Category?.Name ?? "Uncategorized",
                        Spent = spent,
                        BudgetAmount = b.Amount,
                        PercentUsed = b.Amount == 0 ? 0 : Math.Round((double)(spent / b.Amount) * 100, 1),
                        Color = Palette[index % Palette.Length]
                    };
                })
                .OrderByDescending(b => b.PercentUsed)
                .ToList();

            var monthlyTrend = new List<MonthlyTrendPoint>();
            for (var i = 5; i >= 0; i--)
            {
                var monthStart = currentMonthStart.AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1);
                monthlyTrend.Add(new MonthlyTrendPoint
                {
                    Label = monthStart.ToString("MMM"),
                    Income = incomes.Where(x => x.Date >= monthStart && x.Date < monthEnd).Sum(x => x.Amount),
                    Expenses = expenses.Where(x => x.Date >= monthStart && x.Date < monthEnd).Sum(x => x.Amount)
                });
            }

            var notifications = storedNotifications
                .Select(n =>
                {
                    var severity = NotificationDisplay.Severity(n.Title);
                    return new NotificationItem
                    {
                        Icon = NotificationDisplay.Icon(severity),
                        Title = n.Title,
                        Message = n.Message ?? string.Empty,
                        Severity = severity,
                        TimeAgo = TimeAgoHelper.Describe(n.CreatedAt)
                    };
                })
                .ToList();

            var currentUser = await _context.Users.FindAsync(userId);

            var model = new DashboardViewModel
            {
                DisplayName = string.IsNullOrWhiteSpace(currentUser?.FullName)
                    ? (User.Identity?.Name ?? "there")
                    : currentUser!.FullName,
                MonthLabel = now.ToString("MMMM yyyy"),
                CurrentBalance = currentBalance,
                TotalIncome = currentMonthIncome,
                TotalExpenses = currentMonthExpenses,
                TotalSavings = currentMonthSavings,
                BalanceChangePercent = CalcPercentChange(currentBalance, previousBalance),
                IncomeChangePercent = CalcPercentChange(currentMonthIncome, previousMonthIncome),
                ExpenseChangePercent = CalcPercentChange(currentMonthExpenses, previousMonthExpenses),
                SavingsChangePercent = CalcPercentChange(currentMonthSavings, previousMonthSavings),
                MonthlyBudget = monthlyBudget,
                RemainingBudget = remainingBudget,
                RecentTransactions = recentTransactions,
                MonthlyTrend = monthlyTrend,
                ExpenseByCategory = expenseByCategory,
                BudgetProgress = budgetProgress,
                SavingsGoals = savingsGoals,
                Notifications = notifications
            };

            return View(model);
        }

        private static decimal CalcPercentChange(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return current == 0 ? 0 : 100;
            }

            return Math.Round((current - previous) / Math.Abs(previous) * 100, 1);
        }
    }
}
