using PFDETBM.Models;

namespace PFDETBM.ViewModels
{
    public class ReportsViewModel
    {
        public string MonthLabel { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }

        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetSavings { get; set; }

        public List<CategorySlice> IncomeBySource { get; set; } = new();
        public List<CategorySlice> ExpenseByCategory { get; set; } = new();
        public List<BudgetProgressItem> BudgetPerformance { get; set; } = new();
        public List<SavingsGoal> SavingsGoals { get; set; } = new();
    }
}
