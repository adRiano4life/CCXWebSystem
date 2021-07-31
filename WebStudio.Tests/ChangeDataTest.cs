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
    public class ChangeDataTest
    {
        private readonly IWebDriver _driver;
        private readonly UserManager<User> _userManager;
        public BasicSteps _BasicSteps;

        public ChangeDataTest()
        {
            _driver = new ChromeDriver();
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }

        [Fact]
        public void ChangePasswordCorrectDataReturnCorrectChange()
        {
            var db = ReturnsWebStudioDbContext();
            _driver.Navigate().GoToUrl("https://localhost:5001/Account/Login");
            _driver.FindElement(By.Id("email")).SendKeys("test@test.com");
            _driver.FindElement(By.Id("password")).SendKeys("12345Aa");
            _driver.FindElement(By.Id("enter")).Click();
            _driver.FindElement(By.Id("account")).Click();
            _driver.FindElement(By.LinkText("Изменить пароль")).Click();
            
            _driver.FindElement(By.Id("changePassword")).SendKeys("QWEqwe123");
            _driver.FindElement(By.Id("edit")).Click();
            Assert.Contains("Изменение пароля!", _driver.PageSource);
            
            User user = db.Users.FirstOrDefault(u=>u.Name == "test");
            db.Users.Remove(user);
            db.SaveChanges();
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