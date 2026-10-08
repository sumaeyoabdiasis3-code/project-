using Microsoft.EntityFrameworkCore;
using PFDETBM.Data;
using PFDETBM.ViewModels;

namespace PFDETBM.Helpers
{
    public static class TransactionFeedBuilder
    {
        public class Filter
        {
            public string? Search { get; set; }
            public bool? IsExpense { get; set; }
            public string? CategoryName { get; set; }
            public DateTime? From { get; set; }
            public DateTime? To { get; set; }
        }

        public static async Task<List<TransactionFeedItem>> BuildAsync(
            ApplicationDbContext context, string userId, Filter? filter = null, int? take = null)
        {
            var includeIncome = filter?.IsExpense != true;
            var includeExpense = filter?.IsExpense != false;
            var items = new List<TransactionFeedItem>();

            if (includeIncome)
            {
                var incomeQuery = context.Incomes.Where(i => i.UserId == userId);

                if (filter?.From != null) incomeQuery = incomeQuery.Where(i => i.Date >= filter.From);
                if (filter?.To != null) incomeQuery = incomeQuery.Where(i => i.Date <= filter.To);
                if (!string.IsNullOrWhiteSpace(filter?.Search))
                {
                    incomeQuery = incomeQuery.Where(i =>
                        i.Source.Contains(filter.Search) || (i.Description != null && i.Description.Contains(filter.Search)));
                }
                if (!string.IsNullOrWhiteSpace(filter?.CategoryName))
                {
                    incomeQuery = incomeQuery.Where(i => i.Source == filter.CategoryName);
                }

                var incomes = await incomeQuery.ToListAsync();
                items.AddRange(incomes.Select(i => new TransactionFeedItem
                {
                    Date = i.Date,
                    IsExpense = false,
                    Label = i.Source,
                    Description = i.Description,
                    Amount = i.Amount,
                    SourceId = i.Id
                }));
            }

            if (includeExpense)
            {
                var expenseQuery = context.Expenses.Include(e => e.Category).Where(e => e.UserId == userId);

                if (filter?.From != null) expenseQuery = expenseQuery.Where(e => e.Date >= filter.From);
                if (filter?.To != null) expenseQuery = expenseQuery.Where(e => e.Date <= filter.To);
                if (!string.IsNullOrWhiteSpace(filter?.Search))
                {
                    expenseQuery = expenseQuery.Where(e =>
                        (e.Description != null && e.Description.Contains(filter.Search)) ||
                        (e.Category != null && e.Category.Name.Contains(filter.Search)));
                }
                if (!string.IsNullOrWhiteSpace(filter?.CategoryName))
                {
                    expenseQuery = expenseQuery.Where(e => e.Category != null && e.Category.Name == filter.CategoryName);
                }

                var expenses = await expenseQuery.ToListAsync();
                items.AddRange(expenses.Select(e => new TransactionFeedItem
                {
                    Date = e.Date,
                    IsExpense = true,
                    Label = e.Category?.Name ?? "Uncategorized",
                    Description = e.Description,
                    Amount = e.Amount,
                    SourceId = e.Id
                }));
            }

            var ordered = items.OrderByDescending(i => i.Date).ToList();
            return take.HasValue ? ordered.Take(take.Value).ToList() : ordered;
        }
    }
}
