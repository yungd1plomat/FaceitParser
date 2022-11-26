using FaceitParser.Data;
using Microsoft.AspNetCore.Identity;
using System.Xml.Linq;

namespace FaceitParser.Helpers
{
    public static class IdentityDataInitializer
    {
        public static async Task SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedRoles(roleManager);
            await SeedUsers(userManager);
        }

        public static async Task SeedUsers(UserManager<ApplicationUser> userManager)
        {
            var requiredUser = await userManager.FindByNameAsync("admin");
            if (requiredUser is null)
            {
                var user = new ApplicationUser()
                {
                    UserName = "admin",
                    CreateDate = DateTimeOffset.UtcNow,
                };
                var result = await userManager.CreateAsync(user, "Admin_1234");
                await userManager.AddToRoleAsync(user, "admin");
            }
        }

        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("user"))
            {
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole("user"));
            }
            if (!await roleManager.RoleExistsAsync("admin"))
            {
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole("admin"));
            }
        }
    }
}
