using Microsoft.AspNetCore.Identity;
using Testify.Shared.Constants;

namespace Testify.Data
{
    public class UserSeeder
    {
        private static async Task CreateUserWithRole(UserManager<ApplicationUser> userManager, string fullName, string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    FullName = fullName,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    throw new Exception("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await CreateUserWithRole(userManager, "Tom", "admin1@test.com", "Abc123@", UserRole.Admin);
            await CreateUserWithRole(userManager, "Angle", "admin2@test.com", "Abc123@", UserRole.Admin);
            await CreateUserWithRole(userManager, "Anna", "admin3@test.com", "Abc123@", UserRole.Admin);

            await CreateUserWithRole(userManager, "Ethan", "user1@test.com", "Abc123@", UserRole.User);
            await CreateUserWithRole(userManager, "Alex", "user2@test.com", "Abc123@", UserRole.User);
            await CreateUserWithRole(userManager, "Anna", "user3@test.com", "Abc123@", UserRole.User);
        }
    }
}
