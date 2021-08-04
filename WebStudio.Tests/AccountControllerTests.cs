using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using WebStudio.Controllers;
using WebStudio.Models;
using WebStudio.Services;
using WebStudio.ViewModels;
using Xunit;

namespace WebStudio.Tests
{
    public class AccountControllerTests
    {
        private WebStudioContext db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHostEnvironment _environment = new HostingEnvironment();
        private readonly FileUploadService _uploadService = new FileUploadService();
        private ILogger<AccountController> _iLogger;
        
        
        [Fact]
        public async Task RegisterNewUser()
        {
            string DefaultConnection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=QWEqwe123@";
            var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
            var options = optionsBuilder.UseNpgsql(DefaultConnection).Options;
            var _db = new WebStudioContext(options);
            
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

            AccountController controller = new AccountController(_db, _userManager, _roleManager, _signInManager, _environment, _uploadService, _iLogger);

            var result = controller.Register(model);
            
            User user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            Assert.NotNull(user);
            
            /*Assert.Equal(user.Email, model.Email);
            Assert.Equal(user.Name, model.Name);
            Assert.Equal(user.Surname, model.Surname);
            Assert.Equal(user.PhoneNumber, model.PhoneNumber);*/
            
            

        }
    }
}