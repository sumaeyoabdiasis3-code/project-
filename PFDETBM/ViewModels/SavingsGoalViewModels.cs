using System.ComponentModel.DataAnnotations;

namespace PFDETBM.ViewModels
{
    public class SavingsGoalFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0.")]
        [Display(Name = "Target Amount")]
        public decimal TargetAmount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Target Date")]
        public DateTime TargetDate { get; set; } = DateTime.Today.AddMonths(6);
    }

    public class AddSavingsViewModel
    {
        [Required]
        public int GoalId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
