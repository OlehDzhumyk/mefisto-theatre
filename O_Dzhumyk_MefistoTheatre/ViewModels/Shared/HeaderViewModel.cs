namespace O_Dzhumyk_MefistoTheatre.ViewModels.Shared
{
    // ViewModel providing data needed to render the site header (e.g., navigation links, user status).
    public class HeaderViewModel
    {
        // Indicates if the current visitor is authenticated (logged in).
        public bool IsAuthenticated { get; set; }
        // Indicates if the current user has administrative privileges.
        public bool IsAdmin { get; set; }
        // Indicates if the current user has staff privileges.
        public bool IsStaff { get; set; }
        // The full name of the logged-in user, if authenticated.
        public string? UserFullName { get; set; }
    }
}