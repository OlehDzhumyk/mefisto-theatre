using System.ComponentModel.DataAnnotations;

namespace O_Dzhumyk_MefistoTheatre.ViewModels.Profile
{
    // ViewModel for displaying user profile information.
    public class ProfileViewModel
    {
        // The unique identifier of the user (User ID).
        public string Id { get; set; } = string.Empty;

        // The user's full name for display.
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        // The user's email address for display.
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        // Indicates whether the user's account is banned.
        [Display(Name = "Account Status")]
        public bool IsBanned { get; set; }
    }
}