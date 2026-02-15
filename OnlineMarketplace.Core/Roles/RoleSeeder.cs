using Microsoft.AspNetCore.Identity;

namespace OnlineMarketplace.Roles
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(RoleNames.Owner))
            {
                await roleManager.CreateAsync(new IdentityRole(RoleNames.Owner));
            }

            if (!await roleManager.RoleExistsAsync(RoleNames.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(RoleNames.Admin));
            }

            if (!await roleManager.RoleExistsAsync(RoleNames.Customer))
            {
                await roleManager.CreateAsync(new IdentityRole(RoleNames.Customer));
            }
        }
    }
}
