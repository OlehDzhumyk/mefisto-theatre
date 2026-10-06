using System.Net;
using Microsoft.EntityFrameworkCore;
using static O_Dzhumyk_MefistoTheatre.Tests.TestData;

namespace O_Dzhumyk_MefistoTheatre.Tests;

public class AuthTests(MefistoAppFactory app) : IClassFixture<MefistoAppFactory>
{
    [Fact]
    public async Task Login_WithCorrectPassword_RedirectsHome()
    {
        var response = await app.CreateBrowser().LogInAsync(MemberEmail, MemberPassword);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShowsError()
    {
        var response = await app.CreateBrowser().LogInAsync(StaffEmail, "wrong-password");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Invalid login attempt.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_LocksAccountAfterRepeatedFailures()
    {
        var browser = app.CreateBrowser();
        for (var i = 0; i < 5; i++)
            await browser.LogInAsync("member2@example.com", "wrong-password");

        // Even the right password is refused while the account is locked
        var response = await browser.LogInAsync("member2@example.com", "Member@123");

        Assert.Contains("Too many failed attempts", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Register_CreatesMemberAndSignsIn()
    {
        var browser = app.CreateBrowser();

        var response = await browser.SubmitFormAsync("/Auth/Register", "/Auth/Register", new()
        {
            ["Email"] = "new.reader@example.com",
            ["Password"] = "Curtain123",
            ["ConfirmPassword"] = "Curtain123",
            ["FullName"] = "New Reader",
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await browser.GetAsync("/Profile/Me")).StatusCode);
        var roles = await app.WithDbAsync(db =>
            (from u in db.Users
             join ur in db.UserRoles on u.Id equals ur.UserId
             join r in db.Roles on ur.RoleId equals r.Id
             where u.Email == "new.reader@example.com"
             select r.Name).ToListAsync());
        Assert.Equal(["Member"], roles);
    }

    [Fact]
    public async Task Register_RejectsShortPassword()
    {
        var response = await app.CreateBrowser().SubmitFormAsync("/Auth/Register", "/Auth/Register", new()
        {
            ["Email"] = "short@example.com",
            ["Password"] = "abc12",
            ["ConfirmPassword"] = "abc12",
            ["FullName"] = "Short Password",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(await app.WithDbAsync(db => db.Users.AnyAsync(u => u.Email == "short@example.com")));
    }
}
