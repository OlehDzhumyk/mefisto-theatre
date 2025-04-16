using System.ComponentModel.DataAnnotations;

namespace O_Dzhumyk_MefistoTheatre.Models
{
    // Represents a category that can be assigned to Posts.
    public class Category
    {
        // Primary key for the Category entity.
        public int Id { get; set; }

        // The name of the category. Required and limited in length.
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

    }
}