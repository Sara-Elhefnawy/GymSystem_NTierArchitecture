using GymSystem.Infrastructure.Identities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace GymSystem.Infrastructure.Seeders.Identities;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration)
    {
        await EnsureRoleExists(roleManager, RoleNames.SuperAdmin, "Super Admin");
        await EnsureRoleExists(roleManager, RoleNames.Admin, "Admin");

        var superAdminEmail = configuration["IdentityApplicationUserSeed:SuperAdminEmail"] ?? "superadmin@gmail.com";
        var superAdminPassword = configuration["IdentityApplicationUserSeed:SuperAdminPassword"] ?? "SuperAdmin123$";

        var user = await EnsureUserExists(userManager, superAdminEmail, superAdminPassword, RoleNames.SuperAdmin);

        // Debug: Check if password is correct
        var passwordCheck = await userManager.CheckPasswordAsync(user, superAdminPassword);
        Console.WriteLine($"Password check for {superAdminEmail}: {passwordCheck}");

        var adminEmail = configuration["IdentityApplicationUserSeed:AdminEmail"] ?? "admin@gmail.com";
        var adminPassword = configuration["IdentityApplicationUserSeed:AdminPassword"] ?? "Admin123$";

        await EnsureUserExists(userManager, adminEmail, adminPassword, RoleNames.Admin);
    }

    private static async Task<ApplicationUser> EnsureUserExists(
        UserManager<ApplicationUser> userManager,
        string userEmail,
        string userPassword,
        string roleName)
    {
        var user = await userManager.FindByEmailAsync(userEmail);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                FullName = userEmail.Contains("super") ? "Super Admin" : "Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, userPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            Console.WriteLine($"User {userEmail} created successfully");
        }
        else
        {
            Console.WriteLine($"User {userEmail} already exists");

            // If user exists, verify password
            var passwordValid = await userManager.CheckPasswordAsync(user, userPassword);
            Console.WriteLine($"Password valid for {userEmail}: {passwordValid}");

            // If password is invalid, reset it
            if (!passwordValid)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await userManager.ResetPasswordAsync(user, token, userPassword);
                if (resetResult.Succeeded)
                {
                    Console.WriteLine($"Password reset for {userEmail}");
                }
                else
                {
                    Console.WriteLine($"Failed to reset password: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
                }
            }
        }

        // Check if user is already in the role
        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var roleResult = await userManager.AddToRoleAsync(user, roleName);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to add user to role: {errors}");
            }
            Console.WriteLine($"User {userEmail} added to role {roleName}");
        }

        return user;
    }

    private static async Task EnsureRoleExists(RoleManager<ApplicationRole> roleManager, string roleName, string displayName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName,
                DisplayName = displayName
            });

            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to create role");
        }
    }
}
