using System; // Required for DateTime

namespace O_Dzhumyk_MefistoTheatre.ViewModels.Home
{
    // ViewModel representing a single post summary, typically used in lists (like the Blog page).
    // Contains simplified data suitable for display in a feed or list view.
    public class PostViewModel
    {
        // The unique identifier of the post.
        public int Id { get; set; }
        // The title of the post.
        public string Title { get; set; } = string.Empty;
        // A shortened version or summary of the post content.
        public string Content { get; set; } = string.Empty;
        // The full name of the post's author.
        public string AuthorFullName { get; set; } = "Unknown Author";
        // The name of the category the post belongs to.
        public string CategoryName { get; set; } = "Uncategorized";
        // The date and time the post was created.
        public DateTime CreatedAt { get; set; }
    }
}