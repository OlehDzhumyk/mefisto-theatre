
namespace O_Dzhumyk_MefistoTheatre.ViewModels.Home
{
    // ViewModel for the main Blog page, displaying a list of posts with pagination.
    public class BlogViewModel
    {
        // The list of posts to display on the current page. Uses PostViewModel for display data.
        public List<PostViewModel> Posts { get; set; } = new List<PostViewModel>();
        // The title for the Blog page.
        public string PageTitle { get; set; } = string.Empty;
        // The current page number being displayed.
        public int CurrentPage { get; set; }
        // The total number of pages available.
        public int TotalPages { get; set; }
        // Calculated property to determine if a 'Previous' page link should be shown.
        public bool HasPreviousPage => CurrentPage > 1;
        // Calculated property to determine if a 'Next' page link should be shown.
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}