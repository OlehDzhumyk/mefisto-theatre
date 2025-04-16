
namespace O_Dzhumyk_MefistoTheatre.ViewModels.Posts
{
    // ViewModel representing a single comment for display purposes, typically on a post details page.
    public class CommentViewModel
    {
        // The unique identifier of the comment.
        public int Id { get; set; }
        // The text content of the comment.
        public string Content { get; set; } = string.Empty;
        // The full name of the comment's author.
        public string AuthorFullName { get; set; } = string.Empty;
        // The unique identifier of the comment's author (User ID).
        public string AuthorId { get; set; } = string.Empty;
        // The date and time the comment was created.
        public DateTime CreatedAt { get; set; }
        // Flag indicating if the current user has permission to delete this comment.
        public bool CanDelete { get; set; }
    }
}