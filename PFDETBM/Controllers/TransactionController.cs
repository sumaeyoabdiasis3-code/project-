using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PFDETBM.Data;
using PFDETBM.Helpers;
using PFDETBM.ViewModels;

namespace PFDETBM.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        public async Task<IActionResult> Index(string? search, string? type, string? category, DateTime? from, DateTime? to)
        {
            bool? isExpense = type switch
            {
                "income" => false,
                "expense" => true,
                _ => null
            };

            var filter = new TransactionFeedBuilder.Filter
            {
                Search = search,
                IsExpense = isExpense,
                CategoryName = category,
                From = from,
                To = to
            };

            var items = await TransactionFeedBuilder.BuildAsync(_context, CurrentUserId, filter);

            var categoryNames = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => c.Name)
                .ToListAsync();

            var model = new TransactionsIndexViewModel
            {
                Items = items,
                Search = search,
                Type = type,
                CategoryName = category,
                From = from,
                To = to,
                CategoryOptions = categoryNames.Select(n => new SelectListItem { Value = n, Text = n }).ToList()
            };

            return View(model);
        }
    }
}
