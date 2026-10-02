using Microsoft.AspNetCore.Identity;

namespace MediCart.Web.Data
{
    // Seeds roles and starter accounts at startup.
    //
    // Passwords are NOT in the code any more. They come from configuration:
    //   Seed:AdminPassword     (env var on the host: Seed__AdminPassword)
    //   Seed:CustomerPassword  (env var on the host: Seed__CustomerPassword)
    //
    // If a password setting is missing, that group of accounts is simply not created.
    public static class SeedData
    {
        // The passwords this project used to hard-code. They are used ONLY to
        // detect old accounts that still have them, so they can be replaced.
        private const string OldAdminPassword = "Admin@1234";
        private const string OldCustomerPassword = "User@1234";

        private record SeedUser(string FullName, string Email, string? PhoneNumber);

        private static readonly SeedUser[] Admins =
        {
            new("Rahnuma Azra Mahjabin", "rahnuma.medicart@gmail.com", null),
            new("Farzana Mim", "farzana.medicart@gmail.com", null),
            new("Shayma Sharmeen", "shayma.medicart@gmail.com", null),
            new("Zumaina Tahsin", "zumaina.medicart@gmail.com", null),
        };

        private static readonly SeedUser[] Customers =
        {
            new("Zumaina Tahsin", "zumainatahsincat@gmail.com", "01836329304"),
            new("Zumaina Aust", "zumaina.cse.20220204020@aust.edu", "01836329304"),
            new("Zumaina T", "zumaina.t.22@gmail.com", "01836329304"),

            new("Rahnuma Azra Mahjabin", "mahjabin3619@gmail.com", "01909023568"),
            new("Rahnuma Aust", "rahnuma.cse.20230104028@aust.edu", "01909023568"),

            new("Farzana Mim", "farzanamim2535@gmail.com", "01761666732"),
            new("Farzana Aust", "farzana.cse.20230104032@aust.edu", "01761666732"),

            new("Shayma Sharmeen", "sshayma1612@gmail.com", "01798221612"),
            new("Shayma Aust", "shayma.cse.20230104043@aust.edu", "01798221612"),

            new("Sakina Anwar", "sakinaanwar667@gmail.com", "01716367488"),
            new("Farhad Panna", "farhadpannadadijan@gmail.com", "01791719326"),
        };

        public static async Task SeedAsync(
            IServiceProvider services,
            IConfiguration configuration,
            ILogger logger)
        {
            await SeedRolesAsync(services);

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            await SeedUsersAsync(
                userManager, Admins, "Admin",
                configuration["Seed:AdminPassword"], OldAdminPassword,
                "Seed:AdminPassword", logger);

            await SeedUsersAsync(
                userManager, Customers, "Customer",
                configuration["Seed:CustomerPassword"], OldCustomerPassword,
                "Seed:CustomerPassword", logger);
        }

        private static async Task SeedRolesAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in new[] { "Admin", "Customer" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsersAsync(
            UserManager<ApplicationUser> userManager,
            SeedUser[] users,
            string role,
            string? password,
            string oldPassword,
            string settingName,
            ILogger logger)
        {
            var hasPassword = !string.IsNullOrWhiteSpace(password);

            if (!hasPassword)
            {
                logger.LogWarning(
                    "{Setting} is not set: no new {Role} accounts will be created, " +
                    "and old default passwords will not be replaced.",
                    settingName, role);
            }

            foreach (var seed in users)
            {
                var existing = await userManager.FindByEmailAsync(seed.Email);

                // 1) Account does not exist yet -> create it (only if a password is configured).
                if (existing == null)
                {
                    if (!hasPassword) continue;

                    var user = new ApplicationUser
                    {
                        FullName = seed.FullName,
                        PhoneNumber = seed.PhoneNumber,
                        UserName = seed.Email,
                        Email = seed.Email,
                        EmailConfirmed = true
                    };

                    var created = await userManager.CreateAsync(user, password!);

                    if (created.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                    else
                    {
                        logger.LogWarning(
                            "Could not create seed {Role} account {Email}: {Errors}",
                            role, seed.Email,
                            string.Join("; ", created.Errors.Select(e => e.Description)));
                    }

                    continue;
                }

                // 2) Account exists and still uses the old public password -> replace it.
                if (await userManager.CheckPasswordAsync(existing, oldPassword))
                {
                    if (!hasPassword)
                    {
                        logger.LogWarning(
                            "{Email} still uses the old default password. Set {Setting} to replace it.",
                            seed.Email, settingName);
                        continue;
                    }

                    var token = await userManager.GeneratePasswordResetTokenAsync(existing);
                    var reset = await userManager.ResetPasswordAsync(existing, token, password!);

                    if (reset.Succeeded)
                    {
                        logger.LogInformation("Replaced the old default password for {Email}.", seed.Email);
                    }
                    else
                    {
                        logger.LogWarning(
                            "Could not replace the password for {Email}: {Errors}",
                            seed.Email,
                            string.Join("; ", reset.Errors.Select(e => e.Description)));
                    }
                }
            }
        }
    }
}