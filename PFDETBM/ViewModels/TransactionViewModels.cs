using Microsoft.AspNetCore.Mvc.Rendering;

namespace PFDETBM.ViewModels
{
    public class TransactionFeedItem
    {
        public DateTime Date { get; set; }
        public bool IsExpense { get; set; }
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public int SourceId { get; set; }
    }

    public class TransactionsIndexViewModel
    {
        public List<TransactionFeedItem> Items { get; set; } = new();
        public string? Search { get; set; }
        public string? Type { get; set; }
        public string? CategoryName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public List<SelectListItem> CategoryOptions { get; set; } = new();
    }
}
