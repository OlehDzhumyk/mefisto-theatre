using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using O_Dzhumyk_MefistoTheatre.Models;

namespace O_Dzhumyk_MefistoTheatre.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Member> Members { get; set; } // Definition/Use of Member/Staff still unclear from provided code
        public DbSet<Staff> Staffs { get; set; }   // Definition/Use of Member/Staff still unclear from provided code

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure cascade delete for Post -> Comments
            modelBuilder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- CORRECTED Post -> Author Relationship Configuration ---
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)           // Post has one Author (now correctly type User)
                .WithMany(u => u.Posts)         // User has many Posts (using the new inverse property)
                .HasForeignKey(p => p.AuthorId)  // Foreign key in Post table
                .IsRequired()                    // Ensure AuthorId is required
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a User if they have Posts
            // --- End Correction ---

            // Consider configuring other relationships explicitly for clarity if needed
            // e.g., Post <-> Category, Comment <-> User, Comment <-> Post
        }
    }
}