using System;
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
    public class UserEditTest
    {
        private readonly IWebDriver _driver;
        private readonly UserManager<User> _userManager;
        private readonly WebStudioContext _db;
        public User _user;

        public UserEditTest()
        {
            _driver = new ChromeDriver();
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
        

        [Fact]
        public void EditCorrectDataReturnSuccessEdit()
        {
            var db = ReturnsWebStudioDbContext();
            _driver.Navigate().GoToUrl("https://localhost:5001/Account/Login");
            _driver.FindElement(By.Id("email")).SendKeys("test@test.com");
            _driver.FindElement(By.Id("password")).SendKeys("12345Aa");
            _driver.FindElement(By.Id("enter")).Click();
            _driver.FindElement(By.Id("account")).Click();
            _driver.FindElement(By.LinkText("Редактировать")).Click();
            
            _driver.FindElement(By.Id("name")).Clear();
            _driver.FindElement(By.Id("name")).SendKeys("test3");
            _driver.FindElement(By.Id("surname")).Clear();
            _driver.FindElement(By.Id("surname")).SendKeys("test3");
            _driver.FindElement(By.Id("email")).Clear();
            _driver.FindElement(By.Id("email")).SendKeys("test@test.com");
            _driver.FindElement(By.Id("phone")).Clear();
            _driver.FindElement(By.Id("phone")).SendKeys("222222222");
            _driver.FindElement(By.Id("edit")).Click();

            var editUserLink = _driver.FindElement(By.LinkText("Редактировать"));
            var editPasswordLink = _driver.FindElement(By.LinkText("Изменить пароль"));
            var editUserLinkButton = editUserLink.GetAttribute("href");
            var editPasswordLinkButton = editPasswordLink.GetAttribute("href");
            Assert.Contains("Личный кабинет", _driver.PageSource);
            
            User user = db.Users.FirstOrDefault(u=>u.Name == "test3");
            db.Users.Remove(user);
            db.SaveChanges();
        }

        [Fact]
        public void EditEmptyDateReturnsWrongEdit()
        {
            _driver.Navigate().GoToUrl("https://localhost:5001/Account/Login");
            _driver.FindElement(By.Id("email")).SendKeys("admin@admin.com");
            _driver.FindElement(By.Id("password")).SendKeys("Q1w2e3r4t%");
            _driver.FindElement(By.Id("enter")).Click();
            _driver.FindElement(By.Id("account")).Click();
            _driver.FindElement(By.LinkText("Редактировать")).Click();
            
            _driver.FindElement(By.Id("name")).Clear();
            _driver.FindElement(By.Id("name")).SendKeys(String.Empty);
            _driver.FindElement(By.Id("edit")).Click();
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