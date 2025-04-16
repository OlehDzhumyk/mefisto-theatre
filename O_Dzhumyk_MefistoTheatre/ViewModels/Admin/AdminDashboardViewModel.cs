using O_Dzhumyk_MefistoTheatre.Models; // Contains the User model definition

namespace O_Dzhumyk_MefistoTheatre.ViewModels.Admin
{
    // ViewModel for the main admin dashboard, providing a list of users with their roles.
    public class AdminDashboardViewModel
    {
        // List of users, each packaged with their associated roles.
        public List<UserWithRoles> Users { get; set; } = new();
    }

    // Helper class to combine a User object with a list of their role names.
    public class UserWithRoles
    {
        // The user entity.
        public User User { get; set; } = null!;
        // A list of role names assigned to this user (e.g., "Admin", "Member").
        public List<string> Roles { get; set; } = new();
    }
}