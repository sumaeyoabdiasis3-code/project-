using System.ComponentModel.DataAnnotations;

namespace PFDETBM.ViewModels
{
    public class AdminStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalCategories { get; set; }
        public decimal TotalIncomeAllUsers { get; set; }
        public decimal TotalExpensesAllUsers { get; set; }
        public int TotalIncomeEntries { get; set; }
        public int TotalExpenseEntries { get; set; }
        public int TotalBudgets { get; set; }
        public int TotalSavingsGoals { get; set; }
    }

    public class AdminUserRow
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }

    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "This is an expense category")]
        public bool IsExpense { get; set; } = true;
    }
}
