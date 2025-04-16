using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace O_Dzhumyk_MefistoTheatre.Models
{
    // Represents a user in the system, extending the base IdentityUser for authentication features.
    public class User : IdentityUser
    {
        // User's full name is required.
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        // Flag to indicate if the user account is banned. Defaults to false.
        public bool IsBanned { get; set; } = false;

        // Optional field for users to add a description about themselves.
        [StringLength(500)]
        public string? AboutMe { get; set; }

        // Navigation property for comments made by the user. Initialized to an empty list.
        // Made virtual to enable lazy loading.
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Navigation property for posts created by the user. Initialized to an empty list.
        // Made virtual to enable lazy loading.
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}