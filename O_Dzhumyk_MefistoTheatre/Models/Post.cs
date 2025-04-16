using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O_Dzhumyk_MefistoTheatre.Models
{
    // Represents a blog post or article within the application.
    public class Post
    {
        // Primary key for the Post entity.
        public int Id { get; set; }

        // The title of the post, required and limited in length.
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        // The main content of the post, required.
        [Required]
        public string Content { get; set; } = string.Empty;

        // --- Author Relationship ---
        // Foreign key referencing the User who authored the post. Required.
        [Required]
        public string AuthorId { get; set; } = string.Empty;

        // Navigation property back to the User who is the author.
        // The ForeignKey attribute explicitly links AuthorId to this navigation property.
        // Made virtual to enable lazy loading.
        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; } = null!;
        // --- End Author Relationship ---

        // Foreign key referencing the Category this post belongs to. Required.
        [Required]
        public int CategoryId { get; set; }

        // Navigation property back to the Category.
        // Made virtual to enable lazy loading.
        public virtual Category Category { get; set; } = null!;

        // Timestamp when the post was created. Defaults to the current UTC time.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Timestamp when the post was last updated. Nullable if never updated.
        public DateTime? UpdatedAt { get; set; }

        // Navigation property for comments associated with this post. Initialized to an empty list.
        // Made virtual to enable lazy loading.
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}