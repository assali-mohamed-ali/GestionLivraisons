using Identity.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Data;

// SRP: IdentitySeeder only seeds data — no migrations, no context configuration
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var um = services.GetRequiredService<UserManager<ApplicationUser>>();
        var rm = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Seed roles
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await rm.RoleExistsAsync(role))
                await rm.CreateAsync(new IdentityRole(role));
        }

        // Seed admin user
        if (await um.FindByEmailAsync("admin@livraisons.com") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@livraisons.com",
                Email = "admin@livraisons.com",
                FullName = "Administrateur",
                EmailConfirmed = true
            };
            await um.CreateAsync(admin, "Admin123!");
            await um.AddToRoleAsync(admin, "Admin");
        }

        // Seed regular user
        if (await um.FindByEmailAsync("user@livraisons.com") is null)
        {
            var user = new ApplicationUser
            {
                UserName = "user@livraisons.com",
                Email = "user@livraisons.com",
                FullName = "Utilisateur Test",
                EmailConfirmed = true
            };
            await um.CreateAsync(user, "User123!");
            await um.AddToRoleAsync(user, "User");
        }
    }
}
