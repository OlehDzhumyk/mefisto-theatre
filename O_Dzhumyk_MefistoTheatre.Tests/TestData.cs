using Microsoft.EntityFrameworkCore;
using O_Dzhumyk_MefistoTheatre.Models;

namespace O_Dzhumyk_MefistoTheatre.Tests;

/// <summary>Demo accounts created by SeedData, plus helpers for adding posts and comments directly.</summary>
public static class TestData
{
    public const string AdminEmail = "admin@example.com", AdminPassword = "Admin@123";
    public const string StaffEmail = "staff1@example.com", StaffPassword = "Staff@123";
    public const string MemberEmail = "member1@example.com", MemberPassword = "Member@123";
    public const string BannedEmail = "banned1@example.com", BannedPassword = "Banned@123";

    public static Task<Post> AddPostAsync(this MefistoAppFactory app, string title, string content, string? categoryName = null) =>
        app.WithDbAsync(async db =>
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Name == categoryName)
                           ?? new Category { Name = categoryName ?? $"Category {Guid.NewGuid():N}" };
            var post = new Post
            {
                Title = title,
                Content = content,
                Author = await db.Users.SingleAsync(u => u.Email == StaffEmail),
                Category = category,
                CreatedAt = DateTime.UtcNow,
            };
            db.Posts.Add(post);
            await db.SaveChangesAsync();
            return post;
        });

    public static Task<Comment> AddCommentAsync(this MefistoAppFactory app, int postId, string authorEmail, string content) =>
        app.WithDbAsync(async db =>
        {
            var comment = new Comment
            {
                PostId = postId,
                AuthorId = (await db.Users.SingleAsync(u => u.Email == authorEmail)).Id,
                Content = content,
            };
            db.Comments.Add(comment);
            await db.SaveChangesAsync();
            return comment;
        });
}
