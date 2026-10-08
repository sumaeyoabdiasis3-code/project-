using System.ComponentModel.DataAnnotations;

namespace PFDETBM.ViewModels
{
    public class IncomeFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Income Source")]
        public string Source { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
