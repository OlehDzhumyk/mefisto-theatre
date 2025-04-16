using System.ComponentModel.DataAnnotations;

namespace O_Dzhumyk_MefistoTheatre.ViewModels.Posts
{
    // ViewModel used for submitting a new comment on a post.
    public class AddCommentViewModel
    {
        // The ID of the post to which the comment is being added.
        public int PostId { get; set; }
        // The text content of the new comment.
        [Required(ErrorMessage = "Comment content cannot be empty.")]
        public string Content { get; set; } = string.Empty; // Initialize
    }
}