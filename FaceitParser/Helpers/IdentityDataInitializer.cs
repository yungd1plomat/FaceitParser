using FaceitParser.Data;
using FaceitParser.Helpers.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace FaceitParser.Helpers
{
    public static class IdentityDataInitializer
    {
        public static async Task SeedData(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedDoubles(db, userManager);
            await SeedRoles(roleManager);
            await SeedUsers(userManager);
        }

        public static async Task SeedDoubles(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            var users = await userManager.Users.ToListAsync();
            var blacklist = await db.Blacklists.ToListAsync();
            foreach (var user in users)
            {
                var duplicates = blacklist.Where(x => x.UserId == user.Id).GroupBy(a => a.ProfileId).SelectMany(a => a.Skip(1));
                foreach (var dup in duplicates)
                {
                    db.Blacklists.Remove(dup);
                }
            }
            await db.SaveChangesAsync();
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
                await userManager.CreateAsync(user, "Admin_1234");
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
