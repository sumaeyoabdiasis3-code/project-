using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PFDETBM.Models
{
    public class SavingsGoal
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TargetAmount { get; set; }

        [Range(0.00, double.MaxValue)]
        public decimal CurrentAmount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime TargetDate { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public ICollection<SavingsTransaction>? SavingsTransactions { get; set; }
    }
}
