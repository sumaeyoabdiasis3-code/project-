using PFDETBM.Models;

namespace PFDETBM.ViewModels
{
    public class DashboardViewModel
    {
        public string DisplayName { get; set; } = "there";
        public string MonthLabel { get; set; } = string.Empty;

        public decimal CurrentBalance { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalSavings { get; set; }

        public decimal BalanceChangePercent { get; set; }
        public decimal IncomeChangePercent { get; set; }
        public decimal ExpenseChangePercent { get; set; }
        public decimal SavingsChangePercent { get; set; }

        public decimal MonthlyBudget { get; set; }
        public decimal RemainingBudget { get; set; }

        public IList<TransactionFeedItem> RecentTransactions { get; set; } = new List<TransactionFeedItem>();
        public IList<MonthlyTrendPoint> MonthlyTrend { get; set; } = new List<MonthlyTrendPoint>();
        public IList<CategorySlice> ExpenseByCategory { get; set; } = new List<CategorySlice>();
        public IList<BudgetProgressItem> BudgetProgress { get; set; } = new List<BudgetProgressItem>();
        public IList<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();
        public IList<NotificationItem> Notifications { get; set; } = new List<NotificationItem>();
    }

    public class MonthlyTrendPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
    }

    public class CategorySlice
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public double Percent { get; set; }
        public string Color { get; set; } = "#94a3b8";
    }

    public class BudgetProgressItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Spent { get; set; }
        public decimal BudgetAmount { get; set; }
        public double PercentUsed { get; set; }
        public string Color { get; set; } = "#2563eb";
    }

    public class NotificationItem
    {
        public string Icon { get; set; } = "🔔";
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "info";
        public string TimeAgo { get; set; } = "Just now";
    }
}
