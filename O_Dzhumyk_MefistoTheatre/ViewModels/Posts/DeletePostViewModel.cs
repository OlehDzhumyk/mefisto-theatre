namespace O_Dzhumyk_MefistoTheatre.ViewModels.Posts
{
    // ViewModel used for confirming the deletion of a post.
    // Provides minimal information needed for the confirmation view.
    public class DeletePostViewModel
    {
        // The unique identifier of the post to be deleted.
        public int Id { get; set; }
        // The title of the post, displayed for confirmation.
        public string Title { get; set; } = string.Empty;
        // The name of the author, possibly for display in the confirmation message.
        public string AuthorName { get; set; } = string.Empty;
    }
}