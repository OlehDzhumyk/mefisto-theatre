using System;
using System.Collections.Generic;

namespace O_Dzhumyk_MefistoTheatre.ViewModels.Posts
{
    // ViewModel for displaying the full details of a single post, including comments and user actions.
    public class PostDetailsViewModel
    {
        // The unique identifier of the post.
        public int Id { get; set; }
        // The title of the post.
        public string Title { get; set; } = string.Empty;
        // The full content of the post.
        public string Content { get; set; } = string.Empty;
        // The full name of the post's author.
        public string AuthorFullName { get; set; } = string.Empty;
        // The unique identifier of the post's author (User ID).
        public string? AuthorId { get; set; }
        // The name of the category the post belongs to.
        public string CategoryName { get; set; } = string.Empty;
        // The date and time the post was created.
        public DateTime CreatedAt { get; set; }
        // The date and time the post was last updated, if applicable.
        public DateTime? UpdatedAt { get; set; }
        // The list of comments associated with this post, using CommentViewModel for display.
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();

        // --- Permissions and Status Flags ---
        // Determines if the current user can add a comment to this post.
        public bool CanAddComment { get; set; }
        // Determines if the current user can edit this post.
        public bool CanEditPost { get; set; }
        // Determines if the current user can delete this post.
        public bool CanDeletePost { get; set; }
        // Indicates if the current visitor is logged in.
        public bool IsUserAuthenticated { get; set; }
        // Indicates if the current logged-in user's account is suspended or banned.
        public bool IsUserSuspended { get; set; }
    }
}