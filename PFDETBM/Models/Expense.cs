using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PFDETBM.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [StringLength(150)]
        public string PaymentMethod { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
