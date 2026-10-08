using Microsoft.AspNetCore.Identity;

namespace PFDETBM.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public ICollection<Income>? Incomes { get; set; }
        public ICollection<Expense>? Expenses { get; set; }
        public ICollection<Budget>? Budgets { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<SavingsGoal>? SavingsGoals { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
    }
}
