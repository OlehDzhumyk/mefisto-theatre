using System.Net;
using Microsoft.EntityFrameworkCore;
using static O_Dzhumyk_MefistoTheatre.Tests.TestData;

namespace O_Dzhumyk_MefistoTheatre.Tests;

public class PostAndCommentTests(MefistoAppFactory app) : IClassFixture<MefistoAppFactory>
{
    [Fact]
    public async Task HomePage_FiltersByCategory()
    {
        await app.AddPostAsync("Opera night in Leith", "<p>Opera</p>", "Test Opera");
        await app.AddPostAsync("Ballet matinee", "<p>Ballet</p>", "Test Ballet");

        var page = await app.CreateBrowser().GetStringAsync("/?category=Test%20Opera");

        Assert.Contains("Opera night in Leith", page);
        Assert.DoesNotContain("Ballet matinee", page);
    }

    [Fact]
    public async Task PostDetails_StripsScriptsFromContent()
    {
        var post = await app.AddPostAsync("Sneaky post", "<p>Hello</p><script>alert('xss')</script>");

        var page = await app.CreateBrowser().GetStringAsync($"/Posts/Details/{post.Id}");

        Assert.Contains("<p>Hello</p>", page);
        Assert.DoesNotContain("alert('xss')", page);
    }

    [Fact]
    public async Task HomePage_ShowsPlainTextExcerpts()
    {
        await app.AddPostAsync("Excerpt check", "<h2>Heading</h2><p>Body <em>text</em></p>");

        var page = await app.CreateBrowser().GetStringAsync("/");

        Assert.Contains("Heading Body text", page);
    }

    [Fact]
    public async Task Staff_CanCreatePostInNewCategory()
    {
        var browser = app.CreateBrowser();
        await browser.LogInAsync(StaffEmail, StaffPassword);

        var response = await browser.SubmitFormAsync("/Posts/Create", "/Posts/Create", new()
        {
            ["Title"] = "Fringe preview",
            ["Content"] = "<p>Five shows to see this August.</p>",
            ["CategorySelection"] = "new",
            ["NewCategoryName"] = "Festival",
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var post = await app.WithDbAsync(db => db.Posts.Include(p => p.Category).SingleAsync(p => p.Title == "Fringe preview"));
        Assert.Equal("Festival", post.Category.Name);
    }

    [Fact]
    public async Task Member_CanCommentOnPost()
    {
        var post = await app.AddPostAsync("Comment target", "<p>Thoughts?</p>");
        var browser = app.CreateBrowser();
        await browser.LogInAsync(MemberEmail, MemberPassword);

        await browser.SubmitFormAsync($"/Posts/Details/{post.Id}", "/Posts/AddComment", new()
        {
            ["postId"] = post.Id.ToString(),
            ["content"] = "Loved the second act.",
        });

        var page = await browser.GetStringAsync($"/Posts/Details/{post.Id}");
        Assert.Contains("Loved the second act.", page);
    }

    [Fact]
    public async Task BannedUser_CannotComment()
    {
        var post = await app.AddPostAsync("Banned target", "<p>Hi</p>");
        var browser = app.CreateBrowser();
        await browser.LogInAsync(BannedEmail, BannedPassword);

        // A banned user sees no comment form, so take the token from another page
        await browser.SubmitFormAsync("/Profile/Edit", "/Posts/AddComment", new()
        {
            ["postId"] = post.Id.ToString(),
            ["content"] = "Should not appear",
        });

        var count = await app.WithDbAsync(db => db.Comments.CountAsync(c => c.PostId == post.Id));
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Member_CanDeleteOwnComment()
    {
        var post = await app.AddPostAsync("Own comment", "<p>Hi</p>");
        var comment = await app.AddCommentAsync(post.Id, MemberEmail, "Typo, will repost");
        var browser = app.CreateBrowser();
        await browser.LogInAsync(MemberEmail, MemberPassword);

        await browser.SubmitFormAsync($"/Posts/Details/{post.Id}", $"/Posts/DeleteComment/{comment.Id}", new()
        {
            ["postId"] = post.Id.ToString(),
        });

        Assert.False(await app.WithDbAsync(db => db.Comments.AnyAsync(c => c.Id == comment.Id)));
    }

    [Fact]
    public async Task Member_CannotDeleteSomeoneElsesComment()
    {
        var post = await app.AddPostAsync("Other comment", "<p>Hi</p>");
        var comment = await app.AddCommentAsync(post.Id, StaffEmail, "Staff reply");
        var browser = app.CreateBrowser();
        await browser.LogInAsync(MemberEmail, MemberPassword);

        await browser.SubmitFormAsync($"/Posts/Details/{post.Id}", $"/Posts/DeleteComment/{comment.Id}", new()
        {
            ["postId"] = post.Id.ToString(),
        });

        Assert.True(await app.WithDbAsync(db => db.Comments.AnyAsync(c => c.Id == comment.Id)));
    }

    [Fact]
    public async Task Staff_CanDeleteAnyComment()
    {
        var post = await app.AddPostAsync("Moderated", "<p>Hi</p>");
        var comment = await app.AddCommentAsync(post.Id, MemberEmail, "Off-topic");
        var browser = app.CreateBrowser();
        await browser.LogInAsync(StaffEmail, StaffPassword);

        await browser.SubmitFormAsync($"/Posts/Details/{post.Id}", $"/Posts/DeleteComment/{comment.Id}", new()
        {
            ["postId"] = post.Id.ToString(),
        });

        Assert.False(await app.WithDbAsync(db => db.Comments.AnyAsync(c => c.Id == comment.Id)));
    }
}
