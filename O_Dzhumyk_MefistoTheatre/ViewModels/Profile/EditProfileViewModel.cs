using System.ComponentModel.DataAnnotations;

namespace O_Dzhumyk_MefistoTheatre.ViewModels.Profile
{
    // ViewModel for the user profile editing form.
    public class EditProfileViewModel
    {
        // User's full name. Required.
        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        // User's email address. Required and must be a valid email format.
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        // User's banned status (likely read-only display or admin-editable field).
        // Represented as string, might need parsing depending on how it's used.
        public string IsBanned { get; set; } = string.Empty;
    }
}