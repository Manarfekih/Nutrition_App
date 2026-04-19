using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Recipe_Nutrition_App.Models;

namespace Recipe_Nutrition_App.Data;

public static class IdentityDataSeeder
{
    public const string AdminRole = "Admin";
    public const string UserRole = "User";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { AdminRole, UserRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var options = provider.GetRequiredService<IOptions<SeedAdminOptions>>().Value;
        if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
            return;

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var existing = await userManager.FindByEmailAsync(options.Email);
        if (existing is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = options.Email,
            Email = options.Email,
            EmailConfirmed = true
        };

        var create = await userManager.CreateAsync(admin, options.Password);
        if (!create.Succeeded)
            return;

        await userManager.AddToRoleAsync(admin, AdminRole);
    }
}

public sealed class SeedAdminOptions
{
    public const string SectionName = "SeedAdmin";

    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}
