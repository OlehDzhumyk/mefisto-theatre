using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using O_Dzhumyk_MefistoTheatre.Models;
using O_Dzhumyk_MefistoTheatre.ViewModels.Auth;

namespace O_Dzhumyk_MefistoTheatre.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Auth/Register
        public IActionResult Register(string? returnUrl = null)
        {
            // Pass return URL to the view in case of redirection after registration
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Create a new user instance based on the provided registration details
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName
                };

                // Attempt to create the user with the specified password
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Everyone who signs up is a member; staff and admins are promoted from the dashboard
                    await _userManager.AddToRoleAsync(user, "Member");

                    // Sign in the user after successful registration
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    // Redirect to the provided return URL if it's local, otherwise to Home/Index
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }
                // Add errors to the ModelState if user creation failed
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // Return the view with the current model to display validation errors
            return View(model);
        }

        // GET: /Auth/Login
        public IActionResult Login(string? returnUrl = null)
        {
            // Pass return URL to the view for post-login redirection
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Attempt to sign in the user using the provided credentials
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true
                );
                if (result.Succeeded)
                {
                    // Redirect to return URL if valid, otherwise redirect to Home/Index
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }
                // Add an error if login attempt failed
                ModelState.AddModelError(string.Empty, result.IsLockedOut
                    ? "Too many failed attempts. Please try again in a few minutes."
                    : "Invalid login attempt.");
            }
            return View(model);
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign the user out
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/AccessDenied (the cookie middleware sends users here on a 403)
        public IActionResult AccessDenied() => View();
    }
}
