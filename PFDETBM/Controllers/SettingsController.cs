using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PFDETBM.Models;
using PFDETBM.ViewModels;

namespace PFDETBM.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SettingsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            ViewBag.PasswordModel = new ChangePasswordViewModel();

            return View(new ProfileSettingsViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileSettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                model.Email = user.Email ?? string.Empty;
                ViewBag.PasswordModel = new ChangePasswordViewModel();
                return View("Index", model);
            }

            user.FullName = model.FullName;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PasswordModel = model;
                return View("Index", new ProfileSettingsViewModel { FullName = user.FullName ?? string.Empty, Email = user.Email ?? string.Empty });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                ViewBag.PasswordModel = model;
                return View("Index", new ProfileSettingsViewModel { FullName = user.FullName ?? string.Empty, Email = user.Email ?? string.Empty });
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
