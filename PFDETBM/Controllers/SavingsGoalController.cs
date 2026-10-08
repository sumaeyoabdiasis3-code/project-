using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PFDETBM.Data;
using PFDETBM.Models;
using PFDETBM.ViewModels;

namespace PFDETBM.Controllers
{
    [Authorize]
    public class SavingsGoalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SavingsGoalController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        public async Task<IActionResult> Index()
        {
            var goals = await _context.SavingsGoals
                .Where(g => g.UserId == CurrentUserId)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();

            return View(goals);
        }

        public IActionResult Create()
        {
            return View("Form", new SavingsGoalFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavingsGoalFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            var goal = new SavingsGoal
            {
                UserId = CurrentUserId,
                Title = model.Title,
                TargetAmount = model.TargetAmount,
                TargetDate = model.TargetDate,
                CurrentAmount = 0
            };

            _context.SavingsGoals.Add(goal);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Savings goal created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var goal = await _context.SavingsGoals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == CurrentUserId);
            if (goal == null)
            {
                return NotFound();
            }

            var model = new SavingsGoalFormViewModel
            {
                Id = goal.Id,
                Title = goal.Title,
                TargetAmount = goal.TargetAmount,
                TargetDate = goal.TargetDate
            };

            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SavingsGoalFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            var goal = await _context.SavingsGoals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == CurrentUserId);
            if (goal == null)
            {
                return NotFound();
            }

            goal.Title = model.Title;
            goal.TargetAmount = model.TargetAmount;
            goal.TargetDate = model.TargetDate;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Savings goal updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var goal = await _context.SavingsGoals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == CurrentUserId);
            if (goal != null)
            {
                _context.SavingsGoals.Remove(goal);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Savings goal deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSavings(AddSavingsViewModel model)
        {
            var goal = await _context.SavingsGoals.FirstOrDefaultAsync(g => g.Id == model.GoalId && g.UserId == CurrentUserId);
            if (goal == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid || model.Amount <= 0)
            {
                TempData["Error"] = "Enter a valid contribution amount.";
                return RedirectToAction(nameof(Index));
            }

            _context.SavingsTransactions.Add(new SavingsTransaction
            {
                SavingsGoalId = goal.Id,
                UserId = CurrentUserId,
                Amount = model.Amount,
                Date = DateTime.Today,
                Notes = model.Notes
            });

            goal.CurrentAmount += model.Amount;

            if (goal.CurrentAmount >= goal.TargetAmount)
            {
                var alreadyNotified = await _context.Notifications.AnyAsync(n =>
                    n.UserId == CurrentUserId && n.Title == "Savings goal achieved" && n.Message == $"You reached your \"{goal.Title}\" savings goal!");

                if (!alreadyNotified)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = CurrentUserId,
                        Title = "Savings goal achieved",
                        Message = $"You reached your \"{goal.Title}\" savings goal!",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Added {model.Amount:C} to \"{goal.Title}\".";
            return RedirectToAction(nameof(Index));
        }
    }
}
