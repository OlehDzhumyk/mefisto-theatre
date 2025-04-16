using System.ComponentModel.DataAnnotations;

namespace O_Dzhumyk_MefistoTheatre.ViewModels.Auth
{
    // ViewModel representing the data needed for a user login attempt.
    public class LoginViewModel
    {
        // User's email address for login. Required and must be a valid email format.
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // Initialize to avoid null warnings

        // User's password. Required. Data type hint for password masking in UI.
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty; // Initialize to avoid null warnings

        // Option for the user to stay logged in ("Remember Me" cookie).
        public bool RememberMe { get; set; }
    }
}