using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using O_Dzhumyk_MefistoTheatre.Models;
using O_Dzhumyk_MefistoTheatre.ViewModels.Profile;
using System.Security.Claims;

namespace O_Dzhumyk_MefistoTheatre.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;

        public ProfileController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Profile/Me
        public async Task<IActionResult> Me()
        {
            // Retrieve the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Map user data to ProfileViewModel
            var model = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                IsBanned = user.IsBanned
            };

            return View("Profile", model);
        }

        // GET: /Profile/ViewProfile/{id}
        public async Task<IActionResult> ViewProfile(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required.");
            }

            // Redirect to /Profile/Me if viewing own profile
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == id)
            {
                return RedirectToAction(nameof(Me));
            }

            // Find the user by ID
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID '{id}' not found.");
            }

            // Map user data to ProfileViewModel
            var model = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                IsBanned = user.IsBanned
            };

            return View("Profile", model);
        }

        // GET: /Profile/Edit
        public async Task<IActionResult> Edit()
        {
            // Retrieve the current user for editing
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Populate EditProfileViewModel with current user data
            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email
            };

            return View(model);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return view with validation errors
                return View(model);
            }

            // Retrieve the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update user details with provided data
            user.FullName = model.FullName;
            user.UserName = model.Email;
            user.Email = model.Email;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Me));
            }

            // Add errors to the ModelState if update fails
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: /Profile/Ban/{id}
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ban(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required.");
            }

            // Prevent admin from banning their own account
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "You cannot ban your own account.";
                return RedirectToAction(nameof(ViewProfile), new { id });
            }

            // Find user by ID and toggle ban status if not already banned
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID '{id}' not found.");
            }

            if (!user.IsBanned)
            {
                user.IsBanned = true;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"User {user.FullName ?? user.UserName} banned successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to ban user. " + string.Join(" ", result.Errors.Select(e => e.Description));
                }
            }
            else
            {
                TempData["InfoMessage"] = $"User {user.FullName ?? user.UserName} is already banned.";
            }

            return RedirectToAction(nameof(ViewProfile), new { id });
        }

        // POST: /Profile/Unban/{id}
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unban(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required.");
            }

            // Find user by ID and update ban status
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID '{id}' not found.");
            }

            if (user.IsBanned)
            {
                user.IsBanned = false;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"User {user.FullName ?? user.UserName} unbanned successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to unban user. " + string.Join(" ", result.Errors.Select(e => e.Description));
                }
            }
            else
            {
                TempData["InfoMessage"] = $"User {user.FullName ?? user.UserName} is not currently banned.";
            }

            return RedirectToAction(nameof(ViewProfile), new { id });
        }
    }
}
