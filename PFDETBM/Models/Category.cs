using System.ComponentModel.DataAnnotations;

namespace PFDETBM.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public bool IsExpense { get; set; }

        public ICollection<Expense>? Expenses { get; set; }
        public ICollection<Budget>? Budgets { get; set; }
    }
}
