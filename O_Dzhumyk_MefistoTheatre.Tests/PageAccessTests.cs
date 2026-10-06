using System.Net;
using static O_Dzhumyk_MefistoTheatre.Tests.TestData;

namespace O_Dzhumyk_MefistoTheatre.Tests;

public class PageAccessTests(MefistoAppFactory app) : IClassFixture<MefistoAppFactory>
{
    [Theory]
    [InlineData("/")]
    [InlineData("/Home/About")]
    [InlineData("/Home/Contact")]
    [InlineData("/Auth/Login")]
    [InlineData("/Auth/Register")]
    [InlineData("/Posts/Details/1")]
    public async Task PublicPages_LoadForVisitors(string url)
    {
        var response = await app.CreateBrowser().GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnknownPage_ShowsFriendlyNotFoundPage()
    {
        var response = await app.CreateBrowser().GetAsync("/no-such-page");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Page not found", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task OldPostsListLink_RedirectsToHomeWithCategory()
    {
        var response = await app.CreateBrowser().GetAsync("/Posts?category=Reviews");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/?category=Reviews", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("/Admin/Dashboard")]
    [InlineData("/Posts/Create")]
    [InlineData("/Profile/Me")]
    public async Task ProtectedPages_SendVisitorsToLogin(string url)
    {
        var response = await app.CreateBrowser().GetAsync(url);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Auth/Login", response.Headers.Location?.PathAndQuery ?? response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("/Admin/Dashboard")]
    [InlineData("/Posts/Create")]
    public async Task StaffOnlyPages_SendMembersToAccessDenied(string url)
    {
        var browser = app.CreateBrowser();
        await browser.LogInAsync(MemberEmail, MemberPassword);

        var response = await browser.GetAsync(url);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Auth/AccessDenied", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task AdminDashboard_LoadsForAdmin()
    {
        var browser = app.CreateBrowser();
        await browser.LogInAsync(AdminEmail, AdminPassword);

        var response = await browser.GetAsync("/Admin/Dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
