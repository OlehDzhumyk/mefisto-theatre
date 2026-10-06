using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using O_Dzhumyk_MefistoTheatre.Data;
using O_Dzhumyk_MefistoTheatre.Models;
using O_Dzhumyk_MefistoTheatre.Services;

var builder = WebApplication.CreateBuilder(args);

// The connection string comes from appsettings.json locally and from ConnectionStrings__DefaultConnection in Docker.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Same key length the original schema was created with (AddDefaultIdentity sets it implicitly)
    options.Stores.MaxLengthForKeys = 128;
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<PostContentFormatter>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/Home/StatusCode/{0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Bring the database schema up to date, then add roles, demo users and demo posts if they're missing.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // SQL Server uses the migrations; the tests run on SQLite, which builds the schema from the model.
    if (context.Database.IsSqlServer())
        await context.Database.MigrateAsync();
    else
        await context.Database.EnsureCreatedAsync();

    await SeedData.InitializeAsync(
        context,
        services.GetRequiredService<UserManager<User>>(),
        services.GetRequiredService<RoleManager<IdentityRole>>());
}

app.Run();

// Lets the integration tests start the app with WebApplicationFactory<Program>.
public partial class Program { }
