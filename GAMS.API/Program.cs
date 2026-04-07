using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GAMS.API.Data;
using GAMS.API.Models;
using GAMS.API.Services;

var builder = WebApplication.CreateBuilder(args);

// DATABASE
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// IDENTITY with COOKIE AUTH
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit           = false;
    options.Password.RequiredLength         = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// CONFIGURE THE AUTH COOKIE
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name     = "gams_session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite     = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.ExpireTimeSpan      = TimeSpan.FromHours(8);
    options.SlidingExpiration   = true;

    // Return 401/403 JSON for API routes instead of redirecting
    options.Events.OnRedirectToLogin = ctx => {
        if (ctx.Request.Path.StartsWithSegments("/api"))
            ctx.Response.StatusCode = 401;
        else
            ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx => {
        if (ctx.Request.Path.StartsWithSegments("/api"))
            ctx.Response.StatusCode = 403;
        else
            ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});

// FILE UPLOAD SIZE LIMIT
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024);

// SERVICES
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// SEED ROLES AND ADMIN USER ON STARTUP
using (var scope = app.Services.CreateScope())
{
    var db          = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Apply any pending migrations automatically
    await db.Database.MigrateAsync();

    // Seed roles
    foreach (var role in new[] { "Applicant", "SocialWorker", "Admin", "Finance" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    // Seed default admin account
    var adminEmail = "admin@gams.gov.jm";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser {
            UserName = adminEmail, Email = adminEmail, FullName = "System Administrator"
        };
        await userManager.CreateAsync(admin, "Admin@2026!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

// SERVE STATIC FILES from wwwroot (frontend)
app.UseDefaultFiles();   // makes / load index.html
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Fallback: any unmatched route serves index.html (for SPA-style navigation)
app.MapFallbackToFile("index.html");

app.Run();