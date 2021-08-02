using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebStudio.Models;
using Xunit;

namespace WebStudio.Tests
{
    public class BasicSteps
    {
        private readonly IWebDriver _driver;
        private readonly WebStudioContext _db;
        private const string MainPageUrl = "https://localhost:5001";
        private const string LoginPageUrl = MainPageUrl + "/Account/Login";

        public BasicSteps()
        {
            _driver = new ChromeDriver();
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }

        public void GoToUrl(string url)
        {
            _driver.Navigate().GoToUrl(url);
        }

        public void GoToMainPage()
        {
            GoToUrl(MainPageUrl);
        }

        public void GoToLoginPage()
        {
            GoToUrl(LoginPageUrl);
        }

        public void ClickLink(string linkText)
        {
            var link = _driver.FindElement(By.LinkText(linkText));
            link.Click();
        }

        public void ClickButton(string buttonLink)
        {
            var button = _driver.FindElement(By.XPath($"//button[contains(text, '{buttonLink}')]"));
            button.Click();
        }

        public void ClickButtonById(string fieldId)
        {
            var button = _driver.FindElement(By.Id(fieldId));
            button.Click();
        }

        public void FindElementById(string fieldById, string inputText)
        {
            var field = _driver.FindElement(By.Id(fieldById));
            field.SendKeys(inputText);
        }

        public void AddTestUser(string userName, string name, string surname, string email, string phone)
        {
            User user = new User
            {
                UserName = userName,
                Name = name,
                Surname = surname,
                Email = email,
                PhoneNumber = phone,
                EmailConfirmed = true,
                LockoutEnabled = false
            };
            _db.Users.Add(user);
            _db.SaveChanges();
        }

        public void RemoveTestUser(User user)
        {
            _db.Users.Remove(user);
            _db.SaveChanges();
        }
        
        public WebStudioContext ReturnsWebStudioDbContext()
        {
            string DefaultConnection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=QWEqwe123@";
            var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
            var options = optionsBuilder.UseNpgsql(DefaultConnection).Options;
            var db = new WebStudioContext(options);
            return db;
        }
    }
}