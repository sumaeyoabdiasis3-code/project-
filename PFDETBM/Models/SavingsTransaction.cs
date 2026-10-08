using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PFDETBM.Models
{
    public class SavingsTransaction
    {
        public int Id { get; set; }

        [Required]
        public int SavingsGoalId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("SavingsGoalId")]
        public SavingsGoal? SavingsGoal { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
