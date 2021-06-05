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
                    UserName = "admin",
                    AvatarPath = avatarPath
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }
            
            string aselEmail = "asel@isar.kz";
            
            if (await userManager.FindByEmailAsync(aselEmail) is null)
            {
                User asel = new User
                {
                    Email = aselEmail,
                    UserName = "Асель",
                    UserSurname = "Жунусова",
                    PhoneNumber = "8777-111-11-11",
                    AvatarPath = $"/Images/Avatars/defaultavatar.jpg"
                };
                var result = await userManager.CreateAsync(asel, "12345Aa");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(asel, "user");
                }
            }
            
            string erbolEmail = "erbol@isar.kz";
            if (await userManager.FindByEmailAsync(erbolEmail) is null)
            {
                User erbol = new User
                {
                    Email = erbolEmail,
                    UserName = "Ербол",
                    UserSurname = "Нуртас",
                    PhoneNumber = "8777-222-22-22",
                    AvatarPath = $"/Images/Avatars/defaultavatar.jpg"
                };
                var result = await userManager.CreateAsync(erbol, "12345Aa");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(erbol, "user");
                }
            }
            
            string danEmail = "dan@isar.kz";
            if (await userManager.FindByEmailAsync(danEmail) is null)
            {
                User dan = new User
                {
                    Email = danEmail,
                    UserName = "Дэн",
                    UserSurname = "Подорожник",
                    PhoneNumber = "8777-333-33-33",
                    AvatarPath = $"/Images/Avatars/defaultavatar.jpg"
                };
                var result = await userManager.CreateAsync(dan, "12345Aa");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(dan, "user");
                }
            }
        }
        
        
    }
}