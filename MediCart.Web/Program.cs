using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediCart.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Database — PostgreSQL via Neon (connection string from user-secrets)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity — roles enabled, no email confirmation required
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<MediCart.Web.Services.IImageUploadService, MediCart.Web.Services.CloudinaryImageService>();

var app = builder.Build();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var role in new[] { "Admin", "Customer" })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Seed admin accounts
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var admins = new[]
    {
        new { FullName = "Rahnuma Azra Mahjabin", Email = "rahnuma@medicart.com" },
        new { FullName = "Farzana Mim",           Email = "farzana@medicart.com" },
        new { FullName = "Shayma Sharmeen",        Email = "shayma@medicart.com" },
        new { FullName = "Zumaina Tahsin",         Email = "zumaina@medicart.com" },
    };

    foreach (var a in admins)
    {
        if (await userManager.FindByEmailAsync(a.Email) == null)
        {
            var user = new ApplicationUser
            {
                FullName = a.FullName,
                UserName = a.Email,
                Email = a.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Admin@1234");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}

// Seed normal customer accounts
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var customers = new[]
    {
        new { FullName = "Ashfia Newaz", PhoneNumber = "01554154449", Email = "ashfia@gmail.com" },
    };

    foreach (var c in customers)
    {
        if (await userManager.FindByEmailAsync(c.Email) == null)
        {
            var user = new ApplicationUser
            {
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                UserName = c.Email,
                Email = c.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "User@1234");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Customer");
            }
        }
    }
}

// HTTP pipeline
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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();