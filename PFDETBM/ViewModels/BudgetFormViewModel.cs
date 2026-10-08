using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PFDETBM.ViewModels
{
    public class BudgetFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; } = DateTime.Now.Month;

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; } = DateTime.Now.Year;

        public List<SelectListItem> CategoryOptions { get; set; } = new();
    }
}
