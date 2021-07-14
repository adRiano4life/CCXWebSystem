using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Moq;
using WebStudio.Controllers;
using WebStudio.Models;
using WebStudio.Services;
using WebStudio.ViewModels;
using Xunit;

namespace WebStudio.Tests
{
    public class AccountControllerTests
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHostEnvironment _environment;
        private readonly FileUploadService _uploadService;
        
        [Fact]
        public void RegisterNewUser()
        {
            string DefaultConnection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=123";
            var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
            var options = optionsBuilder.UseNpgsql(DefaultConnection).Options;
            var db = new WebStudioContext(options);

            var mock = new Mock<IUserRepository>();

            //AccountController controller = new AccountController(db, _userManager, _roleManager, _signInManager, _environment, _uploadService);
            
            RegisterViewModel model = new RegisterViewModel()
            {
                Email = "Alex_Til@mail.ru",
                Name = "Alex",
                Surname = "Til",
                Password = "12345Aa",
                ConfirmPassword = "12345Aa",
                PhoneNumber = "12345678",
                AvatarPath = $"/Images/Avatars/defaultavatar.jpg",
            };

            //var result = controller.Register(model);
        }
    }
}