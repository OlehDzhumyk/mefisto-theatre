using Microsoft.EntityFrameworkCore;
using O_Dzhumyk_MefistoTheatre.Data;

namespace O_Dzhumyk_MefistoTheatre.Tests;

public class MigrationTests(MefistoAppFactory app) : IClassFixture<MefistoAppFactory>
{
    [Fact]
    public void Model_HasNoChangesMissingFromMigrations()
    {
        // Comparing the model with the migrations snapshot doesn't open a connection, so no server is needed.
        // The app's services are passed in because Identity's key length comes from IdentityOptions in Program.cs.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .UseApplicationServiceProvider(app.Services)
            .Options;
        using var context = new ApplicationDbContext(options);

        Assert.False(context.Database.HasPendingModelChanges(),
            "The model has changed since the last migration. Run `dotnet ef migrations add <Name>`.");
    }
}
