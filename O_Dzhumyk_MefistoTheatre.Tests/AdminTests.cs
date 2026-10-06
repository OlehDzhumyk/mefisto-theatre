using System.Net;
using Microsoft.EntityFrameworkCore;
using static O_Dzhumyk_MefistoTheatre.Tests.TestData;

namespace O_Dzhumyk_MefistoTheatre.Tests;

public class AdminTests(MefistoAppFactory app) : IClassFixture<MefistoAppFactory>
{
    private Task<string> UserIdAsync(string email) =>
        app.WithDbAsync(db => db.Users.Where(u => u.Email == email).Select(u => u.Id).SingleAsync());

    private async Task<HttpClient> LoggedInAdminAsync()
    {
        var browser = app.CreateBrowser();
        await browser.LogInAsync(AdminEmail, AdminPassword);
        return browser;
    }

    [Fact]
    public async Task Admin_CanPromoteMemberToStaff()
    {
        var browser = await LoggedInAdminAsync();
        var userId = await UserIdAsync("member3@example.com");

        await browser.SubmitFormAsync("/Admin/Dashboard", "/Admin/PromoteUser", new()
        {
            ["userId"] = userId,
            ["role"] = "Staff",
        });

        var isStaff = await app.WithDbAsync(db =>
            db.UserRoles.AnyAsync(ur => ur.UserId == userId && db.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Staff")));
        Assert.True(isStaff);
    }

    [Fact]
    public async Task PromoteUser_RejectsUnknownRole()
    {
        var browser = await LoggedInAdminAsync();

        var response = await browser.SubmitFormAsync("/Admin/Dashboard", "/Admin/PromoteUser", new()
        {
            ["userId"] = await UserIdAsync("member4@example.com"),
            ["role"] = "SuperUser",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanBanAndUnbanUser()
    {
        var browser = await LoggedInAdminAsync();
        var userId = await UserIdAsync("member5@example.com");
        Task<bool> IsBanned() => app.WithDbAsync(db => db.Users.Where(u => u.Id == userId).Select(u => u.IsBanned).SingleAsync());

        await browser.SubmitFormAsync("/Admin/Dashboard", "/Admin/BanUser", new() { ["userId"] = userId });
        Assert.True(await IsBanned());

        await browser.SubmitFormAsync("/Admin/Dashboard", "/Admin/UnbanUser", new() { ["userId"] = userId });
        Assert.False(await IsBanned());
    }

    [Fact]
    public async Task Admin_CannotBanThemselves()
    {
        var browser = await LoggedInAdminAsync();
        var adminId = await UserIdAsync(AdminEmail);

        await browser.SubmitFormAsync("/Admin/Dashboard", "/Admin/BanUser", new() { ["userId"] = adminId });

        Assert.False(await app.WithDbAsync(db => db.Users.Where(u => u.Id == adminId).Select(u => u.IsBanned).SingleAsync()));
    }
}
