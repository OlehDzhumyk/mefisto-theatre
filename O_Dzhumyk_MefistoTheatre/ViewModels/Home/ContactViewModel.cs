namespace O_Dzhumyk_MefistoTheatre.ViewModels.Home
{
    // ViewModel for the 'Contact' page, providing contact information.
    public class ContactViewModel
    {
        // The title for the Contact page.
        public string PageTitle { get; set; } = string.Empty;
        // The physical address or location information.
        public string Address { get; set; } = string.Empty;
        // Contact phone number.
        public string Phone { get; set; } = string.Empty;
        // Contact email address.
        public string Email { get; set; } = string.Empty;
    }
}