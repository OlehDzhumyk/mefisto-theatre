using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for ForeignKey attribute

namespace O_Dzhumyk_MefistoTheatre.Models
{
    // Represents a comment made on a Post.
    public class Comment
    {
        // Primary key for the Comment entity.
        public int Id { get; set; }

        // The text content of the comment. Required.
        [Required]
        public string Content { get; set; } = string.Empty;

        // Foreign key referencing the User who wrote the comment. Required.
        [Required]
        public string AuthorId { get; set; } = string.Empty;

        // Navigation property back to the User (Author).
        // Consider making this virtual for lazy loading consistency.
        [ForeignKey("AuthorId")] // Explicitly links AuthorId to the Author navigation property
        public User Author { get; set; } = null!;

        // Foreign key referencing the Post this comment belongs to. Required.
        [Required]
        public int PostId { get; set; }

        // Navigation property back to the Post.
        // Consider making this virtual for lazy loading consistency.
        [ForeignKey("PostId")] // Explicitly links PostId to the Post navigation property
        public Post Post { get; set; } = null!;

        // Timestamp when the comment was created. Defaults to the current UTC time.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Timestamp when the comment was last updated. Nullable if never updated.
        public DateTime? UpdatedAt { get; set; }
    }
}