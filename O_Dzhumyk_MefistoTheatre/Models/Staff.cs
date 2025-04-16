using System.Collections.Generic;

namespace O_Dzhumyk_MefistoTheatre.Models
{
    // Represents a staff member, inheriting properties from the base User class.
    // Staff might have additional permissions or properties specific to their role.
    public class Staff : User
    {
        // Currently includes a list of posts managed or relevant to the staff member.
        public List<Post> Posts { get; set; } = new();
    }
}