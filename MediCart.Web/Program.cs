using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;
using MediCart.Web.Services.Ai;
using MediCart.Web.Services.Ai.Tools;

var builder = WebApplication.CreateBuilder(args);

// Database — PostgreSQL via Neon (connection string from user-secrets)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Hosting behind a proxy (Render): read the real visitor IP and the original
// https scheme from the X-Forwarded-* headers the proxy adds.
// The rate limiter below needs the real IP, otherwise all guests share one bucket.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Render's proxy addresses are not fixed, so trust the proxy in front of us.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Keep the keys that protect login cookies and antiforgery tokens in the
// database. On Render the disk is wiped on every restart, which would
// otherwise log everyone out each time.
builder.Services.AddDataProtection()
    .SetApplicationName("MediCart")
    .PersistKeysToDbContext<ApplicationDbContext>();

// Identity — roles enabled, no email confirmation required
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<
    MediCart.Web.Services.IImageUploadService,
    MediCart.Web.Services.CloudinaryImageService>();

builder.Services.AddScoped<
    MediCart.Web.Services.ICartService,
    MediCart.Web.Services.CartService>();

builder.Services.AddScoped<
    MediCart.Web.Services.IOrderService,
    MediCart.Web.Services.OrderService>();

builder.Services.AddScoped<
    MediCart.Web.Services.IEmailService,
    MediCart.Web.Services.EmailService>();

builder.Services.AddHostedService<
    MediCart.Web.Services.CartExpiryBackgroundService>();

// AI assistant (Groq) — key comes from user-secrets: Groq:ApiKey
builder.Services.Configure<GroqOptions>(
    builder.Configuration.GetSection(GroqOptions.SectionName));

builder.Services.AddHttpClient<IGroqClient, GroqClient>();

builder.Services.AddScoped<IAiChatService, AiChatService>();

// AI tools.
// Each tool declares which roles may use it.
builder.Services.AddScoped<IAiTool, AdminAttentionSummaryTool>();
builder.Services.AddScoped<IAiTool, MedicineSearchTool>();
builder.Services.AddScoped<IAiTool, CustomerOrderTrackingTool>();

// Rate limit for the chat endpoint so nobody can burn the Groq quota.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("ai-chat", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.IsAuthenticated == true
                ? $"user:{httpContext.User.Identity.Name}"
                : $"ip:{httpContext.Connection.RemoteIpAddress}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 15,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// Seed roles and starter accounts.
// Account passwords come from configuration (Seed:AdminPassword and
// Seed:CustomerPassword), never from the code. See Data/SeedData.cs.
using (var scope = app.Services.CreateScope())
{
    await SeedData.SeedAsync(scope.ServiceProvider, app.Configuration, app.Logger);
}

// HTTP pipeline
// Must come first so every later step sees the real client IP and scheme.
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();