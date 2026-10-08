using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PFDETBM.Models
{
    public class Budget
    {
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.00, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
