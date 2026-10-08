using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PFDETBM.Data;
using PFDETBM.Models;
using PFDETBM.ViewModels;

namespace PFDETBM.Controllers
{
    [Authorize]
    public class IncomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Incomes.Where(i => i.UserId == CurrentUserId);
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => i.Source.Contains(search) || (i.Description != null && i.Description.Contains(search)));
            }

            var incomes = await query.OrderByDescending(i => i.Date).ToListAsync();
            ViewData["Search"] = search;
            ViewData["Total"] = incomes.Sum(i => i.Amount);
            return View(incomes);
        }

        public IActionResult Create()
        {
            return View("Form", new IncomeFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IncomeFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            var income = new Income
            {
                UserId = CurrentUserId,
                Source = model.Source,
                Amount = model.Amount,
                Date = model.Date,
                Description = model.Description
            };

            _context.Incomes.Add(income);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Income added successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var income = await _context.Incomes.FirstOrDefaultAsync(i => i.Id == id && i.UserId == CurrentUserId);
            if (income == null)
            {
                return NotFound();
            }

            var model = new IncomeFormViewModel
            {
                Id = income.Id,
                Source = income.Source,
                Amount = income.Amount,
                Date = income.Date,
                Description = income.Description
            };

            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IncomeFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            var income = await _context.Incomes.FirstOrDefaultAsync(i => i.Id == id && i.UserId == CurrentUserId);
            if (income == null)
            {
                return NotFound();
            }

            income.Source = model.Source;
            income.Amount = model.Amount;
            income.Date = model.Date;
            income.Description = model.Description;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Income updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var income = await _context.Incomes.FirstOrDefaultAsync(i => i.Id == id && i.UserId == CurrentUserId);
            if (income != null)
            {
                _context.Incomes.Remove(income);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Income deleted.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
