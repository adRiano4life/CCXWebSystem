using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebStudio.Models;
using Xunit;

namespace WebStudio.Tests
{
    public class UserRegisterTest
    {
        private readonly IWebDriver _driver;
        private readonly UserManager<User> _userManager; 
        //private readonly WebStudioContext _db;

        public UserRegisterTest()
        {
            _driver = new ChromeDriver();
        }
        
        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }

        [Fact]
        public void RegisterUserCorrectDataReturnSuccessRegister()
        {
            var db = ReturnsWebStudioDbContext();
            _driver.Navigate().GoToUrl("https://localhost:5001/Account/Register");
            _driver.FindElement(By.Id("name")).SendKeys("test");
            _driver.FindElement(By.Id("surname")).SendKeys("test");
            _driver.FindElement(By.Id("email")).SendKeys("test@test.com");
            _driver.FindElement(By.Id("phone")).SendKeys("111111111");
            _driver.FindElement(By.Id("password")).SendKeys("12345Aa");
            _driver.FindElement(By.Id("confirmPassword")).SendKeys("12345Aa");
            _driver.FindElement(By.Id("register")).Click();
            
            var editUserLink = _driver.FindElement(By.LinkText("Редактировать"));
            var editPasswordLink = _driver.FindElement(By.LinkText("Изменить пароль"));
            var editUserLinkButton = editUserLink.GetAttribute("href");
            var editPasswordLinkButton = editPasswordLink.GetAttribute("href");
            Assert.Contains(
                "Для использования приложения активируйте свою учетную запись пройдя по ссылке, отправленной на вашу эл.почту",
                _driver.PageSource);
            
            User user = db.Users.FirstOrDefault(u => u.Name == "test");
            user.LockoutEnabled = false;
            user.EmailConfirmed = true;
            db.SaveChanges();
        }

        [Fact]
        public void RegisterWrongConfirmPasswordDataReturnWrongRegister()
        {
            _driver.Navigate().GoToUrl("https://localhost:5001/Account/Register");
            _driver.FindElement(By.Id("name")).SendKeys("test2");
            _driver.FindElement(By.Id("surname")).SendKeys("test2");
            _driver.FindElement(By.Id("email")).SendKeys("test2@test.com");
            _driver.FindElement(By.Id("phone")).SendKeys("111111111");
            _driver.FindElement(By.Id("password")).SendKeys("12345Aa");
            _driver.FindElement(By.Id("confirmPassword")).SendKeys("12345A");
            _driver.FindElement(By.Id("register")).Click();
            
            Assert.Contains("Пароли не совпадают", _driver.PageSource);
        }
        
        [Fact]
        public void RegisterEmptyNameDataReturnWrongRegister()
        {
            _driver.Navigate().GoToUrl("https://localhost:5001/Account/Register");
            _driver.FindElement(By.Id("name")).SendKeys(string.Empty);
            _driver.FindElement(By.Id("surname")).SendKeys("test2");
            _driver.FindElement(By.Id("email")).SendKeys("test2@test.com");
            _driver.FindElement(By.Id("phone")).SendKeys("111111111");
            _driver.FindElement(By.Id("password")).SendKeys("12345Aa");
            _driver.FindElement(By.Id("confirmPassword")).SendKeys("12345A");
            _driver.FindElement(By.Id("register")).Click();
            
            Assert.Contains("Это поле обязательно для заполнения", _driver.PageSource);
        }
        
        [NonAction]
        private WebStudioContext ReturnsWebStudioDbContext()
        {
            string DefaultConnection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=QWEqwe123@";
            var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
            var options = optionsBuilder.UseNpgsql(DefaultConnection).Options;
            var db = new WebStudioContext(options);
            return db;
        }
    }
}