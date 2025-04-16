using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using O_Dzhumyk_MefistoTheatre.Data;
using O_Dzhumyk_MefistoTheatre.Models;
using O_Dzhumyk_MefistoTheatre.ViewModels.Admin;

namespace O_Dzhumyk_MefistoTheatre.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Retrieve all users from the database
            var users = await _userManager.Users.ToListAsync();
            var usersWithRoles = new List<UserWithRoles>();

            // Map each user to a view model including their roles
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRoles { User = user, Roles = roles.ToList() });
            }

            var viewModel = new AdminDashboardViewModel
            {
                Users = usersWithRoles
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string userId)
        {
            // Find the user by their ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Prevent an admin from banning themselves
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (user.Id != currentUserId)
                {
                    user.IsBanned = true;
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string userId)
        {
            // Find the user by their ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Prevent an admin from unbanning themselves
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (user.Id != currentUserId)
                {
                    user.IsBanned = false;
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteUser(string userId, string role)
        {
            // Find the user by their ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Only add the user to the role if they are not already in it
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    var result = await _userManager.AddToRoleAsync(user, role);
                    // Optionally, handle the failure of role assignment if necessary
                    if (!result.Succeeded)
                    {
                        // Error handling can be added here
                    }
                }
            }
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
