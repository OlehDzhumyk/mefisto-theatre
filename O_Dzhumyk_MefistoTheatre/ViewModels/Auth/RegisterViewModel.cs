using System.ComponentModel.DataAnnotations;

namespace O_Dzhumyk_MefistoTheatre.ViewModels.Auth
{
    // ViewModel representing the data needed for a new user registration.
    public class RegisterViewModel
    {
        // User's email address. Required and must be a valid email format.
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // Initialize

        // User's chosen password. Required, with length constraints. Data type hint for UI.
        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty; // Initialize

        // Password confirmation field. Must match the Password field. Data type hint for UI.
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty; // Initialize

        // User's full name. Required, with length constraint.
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty; // Initialize
    }
}