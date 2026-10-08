using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PFDETBM.Models;

namespace PFDETBM.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Income> Incomes => Set<Income>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<Budget> Budgets => Set<Budget>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
        public DbSet<SavingsTransaction> SavingsTransactions => Set<SavingsTransaction>();
        public DbSet<Notification> Notifications => Set<Notification>();
    }
}
