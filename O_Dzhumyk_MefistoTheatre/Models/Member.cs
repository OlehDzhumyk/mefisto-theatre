using System;

namespace O_Dzhumyk_MefistoTheatre.Models
{
    // Represents a standard registered member, inheriting properties from the base User class.
    public class Member : User
    {
        // Timestamp indicating when the member registered. Defaults to the current UTC time.
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Other member-specific properties could be added here if needed.
    }
}