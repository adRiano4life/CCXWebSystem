using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WebStudio.Models;

namespace WebStudio.Services
{
    public class RoleInitializer
    {
        public static async Task Initializer(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            string adminEmail = "admin@admin.com";
            string adminPassword = "Q1w2e3r4t%";
            string avatarPath = $"/Images/Avatars/defaultavatar.jpg";

            string userEmail = "user@user.com";
            string userPassword = "12345Aa";
            
            string userEmail2 = "user2@user.com";
            string userPassword2 = "12345Aa";
            
            string userEmail3 = "user3@user.com";
            string userPassword3 = "12345Aa";

            var roles = new[] {"admin", "user"};

            foreach (var role in roles)
            {
                if (await roleManager.FindByNameAsync(role) is null)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                User admin = new User
                {
                    Email = adminEmail,
                    UserName = adminEmail,
                    Name = "admin",
                    Surname = "admin",
                    AvatarPath = avatarPath,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }

            if (await userManager.FindByEmailAsync(userEmail) is null)
            {
                User user = new User
                {
                    Email = userEmail,
                    UserName = userEmail,
                    Name = "Доминик",
                    Surname = "Торрето",
                    AvatarPath = avatarPath,
                    RoleDisplay = "user",
                    EmailConfirmed = true
                };
                var userResult = await userManager.CreateAsync(user, userPassword);
                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "user");
                }
            }
            
            if (await userManager.FindByEmailAsync(userEmail2) is null)
            {
                User user = new User
                {
                    Email = userEmail2,
                    UserName = userEmail2,
                    Name = "Брайн",
                    Surname = "ОКоннер",
                    AvatarPath = avatarPath,
                    RoleDisplay = "user",
                    EmailConfirmed = true
                };
                var userResult = await userManager.CreateAsync(user, userPassword);
                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "user");
                }
            }
            
            if (await userManager.FindByEmailAsync(userEmail3) is null)
            {
                User user = new User
                {
                    Email = userEmail3,
                    UserName = userEmail3,
                    Name = "Ромео",
                    Surname = "Сантос",
                    AvatarPath = avatarPath,
                    RoleDisplay = "user",
                    EmailConfirmed = true
                };
                var userResult = await userManager.CreateAsync(user, userPassword);
                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "user");
                }
            }
        }
        
        
    }
}