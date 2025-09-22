// Mars.Admin/Program.cs
// EF Core only for Identity; connection string comes from appsettings.*.json / secrets / env vars.
// Adds a startup log that shows environment + DB target and tests connectivity.

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mars.Admin.Components;
using Mars.Admin.Components.Account;
using Mars.Admin.Data;
using Mars.Admin.Data.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// 1) Read the connection string from the standard configuration chain.
//    (appsettings.json -> appsettings.{Environment}.json -> UserSecrets (Dev) -> Environment Variables)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Check appsettings.*.json, user secrets, or environment variables.");

// 2) EF Core for Identity only. No app tables here — those will be Dapper later via Mars.Admin.Data.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3) Helpful EF error pages during development (migrations, etc.)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 4) Identity using ApplicationDbContext as the store.
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // I prefer confirmed accounts; tweak as needed.
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// 5) Blazor + Identity plumbing (standard template bits).
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
}).AddIdentityCookies();

// 6) Scoped services for the app (ready for Dapper later)
builder.Services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// --- Custom startup logging (environment + DB info + connectivity test) ---
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var env = app.Environment.EnvironmentName;

    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var conn = dbContext.Database.GetDbConnection();

        logger.LogInformation("---------------------------------------------------");
        logger.LogInformation(" ASP.NET Core Environment: {Environment}", env);
        logger.LogInformation(" Database Provider: {Provider}", dbContext.Database.ProviderName);
        logger.LogInformation(" Connection Target: {Server} / {Database}", conn.DataSource, conn.Database);

        await conn.OpenAsync();
        logger.LogInformation(" Database connection successful");
        await conn.CloseAsync();
        logger.LogInformation("---------------------------------------------------");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, " Database connection failed!");
    }
}
// --- End custom startup logging ---

// Pipeline
if (app.Environment.IsDevelopment())
{
    // Dev-only migration endpoints & detailed EF pages
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Identity endpoints (e.g., /Account/*) from the Identity UI package.
app.MapAdditionalIdentityEndpoints();

// Minimal DB health endpoint: returns 200 OK with "OK" if the connection opens,
// otherwise 503 with "UNHEALTHY: <ExceptionType> - <Message>".
app.MapGet("/health/db", async (IConfiguration cfg) =>
{
    var cs = cfg.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(cs))
        return Results.Text("UNHEALTHY: Missing connection string 'DefaultConnection'",
            "text/plain", System.Text.Encoding.UTF8, statusCode: 503);

    try
    {
        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();
        await conn.CloseAsync();
        return Results.Text("OK", "text/plain"); // 200
    }
    catch (Exception ex)
    {
        return Results.Text($"UNHEALTHY: {ex.GetType().Name} - {ex.Message}",
            "text/plain", System.Text.Encoding.UTF8, statusCode: 503);
    }
});


// ---------------------------------------------------------------------

app.Run();
