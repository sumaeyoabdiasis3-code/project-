using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PFDETBM.ViewModels
{
    public class ExpenseFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Cash";

        [StringLength(500)]
        public string? Description { get; set; }

        public List<SelectListItem> CategoryOptions { get; set; } = new();
    }
}
