using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Required for SelectListItem

namespace O_Dzhumyk_MefistoTheatre.ViewModels.Posts
{
    // Enum to define whether the form is for creating a new post or editing an existing one.
    public enum FormMode
    {
        Create,
        Edit
    }

    // ViewModel for the post creation and editing form.
    public class PostFormViewModel
    {
        // Post ID. If > 0, indicates editing mode; otherwise, creating mode.
        public int Id { get; set; }

        // Post title. Required, with length constraint.
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        // Post content. Required.
        [Required]
        public string Content { get; set; } = string.Empty;

        // Holds the selected value from the category dropdown.
        // Can be an existing category ID or a special value (e.g., "new") to indicate creating a new category.
        [Display(Name = "Category")]
        public string? CategorySelection { get; set; }

        // Holds the name for a new category if CategorySelection indicates creation.
        [Display(Name = "New Category Name")]
        public string? NewCategoryName { get; set; }

        // List of available categories for the dropdown, including an option to add a new one.
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        // Indicates whether the form is in 'Create' or 'Edit' mode.
        public FormMode Mode { get; set; }

        // Provides the appropriate text for the submit button based on the form mode.
        public string SubmitText => Mode == FormMode.Edit ? "Save Changes" : "Create Post";

        // Provides the appropriate title for the form page based on the form mode.
        public string PageTitle => Mode == FormMode.Edit ? "Edit Post" : "Create New Post";
    }
}