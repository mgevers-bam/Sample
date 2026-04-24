using Authentication.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Application.Api.Seeding;

public static class TestUserSeeder
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
    {
        const string testEmail = "admin@stargate.com";
        const string testUserName = "admin";
        const string testPassword = "Password123";

        var existingUser = await userManager.FindByEmailAsync(testEmail);
        if (existingUser is null)
        {
            var testUser = new ApplicationUser
            {
                UserName = testUserName,
                Email = testEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(testUser, testPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create test user: {errors}");
            }
        }
    }
}
