using Microsoft.EntityFrameworkCore;
using PFDETBM.Data;
using PFDETBM.Models;

namespace PFDETBM.Helpers
{
    public static class BudgetAlertGenerator
    {
        public static async Task CheckAndNotifyAsync(ApplicationDbContext context, string userId, int categoryId, int month, int year)
        {
            var budget = await context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.UserId == userId && b.CategoryId == categoryId && b.Month == month && b.Year == year);

            if (budget == null || budget.Amount <= 0)
            {
                return;
            }

            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var spent = await context.Expenses
                .Where(e => e.UserId == userId && e.CategoryId == categoryId && e.Date >= monthStart && e.Date < monthEnd)
                .SumAsync(e => e.Amount);

            var percent = spent / budget.Amount * 100;
            var categoryName = budget.Category?.Name ?? "this category";

            string? title = null;
            string? message = null;

            if (percent >= 100)
            {
                title = "Budget exceeded";
                message = $"You have exceeded your {categoryName} budget for {monthStart:MMMM yyyy}.";
            }
            else if (percent >= 80)
            {
                title = "Approaching budget limit";
                message = $"You are close to your {categoryName} budget limit for {monthStart:MMMM yyyy}.";
            }

            if (title == null)
            {
                return;
            }

            var alreadyNotified = await context.Notifications.AnyAsync(n =>
                n.UserId == userId && n.Title == title && n.Message == message);

            if (alreadyNotified)
            {
                return;
            }

            context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await context.SaveChangesAsync();
        }
    }
}
