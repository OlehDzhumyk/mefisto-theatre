using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using O_Dzhumyk_MefistoTheatre.Data;

namespace O_Dzhumyk_MefistoTheatre.Tests;

/// <summary>
/// Runs the real app in memory, with SQL Server swapped for an in-memory SQLite database.
/// Program.cs builds the schema and seeds the demo users and posts on start-up, as it does in Docker.
/// </summary>
public class MefistoAppFactory : WebApplicationFactory<Program>
{
    // An in-memory SQLite database lives as long as its connection stays open
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

            _connection.Open();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public HttpClient CreateBrowser() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    /// <summary>Runs code against the app's database, e.g. to add test data or check the result of a request.</summary>
    public async Task<T> WithDbAsync<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}

public static partial class BrowserExtensions
{
    /// <summary>Logs in through the real login form, so the client keeps the auth cookie.</summary>
    public static async Task<HttpResponseMessage> LogInAsync(this HttpClient client, string email, string password) =>
        await client.SubmitFormAsync("/Auth/Login", "/Auth/Login", new()
        {
            ["Email"] = email,
            ["Password"] = password,
        });

    /// <summary>Opens <paramref name="pageUrl"/>, takes its antiforgery token and posts the form to <paramref name="action"/>.</summary>
    public static async Task<HttpResponseMessage> SubmitFormAsync(
        this HttpClient client, string pageUrl, string action, Dictionary<string, string> fields)
    {
        var page = await client.GetStringAsync(pageUrl);
        var token = TokenPattern().Match(page);
        if (!token.Success) throw new InvalidOperationException($"No antiforgery token on {pageUrl}");

        fields["__RequestVerificationToken"] = WebUtility.HtmlDecode(token.Groups[1].Value);
        return await client.PostAsync(action, new FormUrlEncodedContent(fields));
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenPattern();
}
