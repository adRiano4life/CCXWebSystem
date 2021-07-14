using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace WebStudio.Tests
{
    public class UserAuthTest
    {
        private readonly IWebDriver _driver;
        private readonly BasicSteps _basicSteps;

        public UserAuthTest()
        {
            _basicSteps = new BasicSteps();
            _driver = new ChromeDriver();
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }

        [Fact]
        public void LoginCorrectDataReturnsSuccessAuth()
        {
            _driver.Navigate().GoToUrl("https://localhost:5001/Account/Login");
            _driver.FindElement(By.Id("email")).SendKeys("admin@admin.com");
            _driver.FindElement(By.Id("password")).SendKeys("Q1w2e3r4t%");
            _driver.FindElement(By.Id("enter")).Click();

            var suppliersRequestsButton = _driver.FindElement(By.LinkText("Запросы поставщикам"));
            var suppliersDataBaseButton = _driver.FindElement(By.LinkText("База поставщиков"));
            var suppliersRequestsButtonLink = suppliersRequestsButton.GetAttribute("href");
            var suppliersDataBaseButtonLink = suppliersDataBaseButton.GetAttribute("href");
            Assert.Contains("Выход", _driver.PageSource);
            Assert.Equal("https://localhost:5001/Requests", suppliersRequestsButtonLink);
            Assert.Equal("https://localhost:5001/Suppliers", suppliersDataBaseButtonLink);
        }
        
        [Fact]
        public void LoginWrongDataReturnsErrorMessage()
        {
            _driver.Navigate().GoToUrl("https://localhost:5001/Account/Login");
            _driver.FindElement(By.Id("email")).SendKeys("wrong@email.com");
            _driver.FindElement(By.Id("password")).SendKeys("wrongPassword");
            _driver.FindElement(By.Id("enter")).Click();
            Assert.Contains("Данный пользователь не зарегистрирован в системе", _driver.PageSource);
        }
        
        [Fact]
        public void LoginEmptyDataReturnsErrorMessage()
        {
            _driver.Navigate().GoToUrl("https://localhost:5001/Account/Login");
            _driver.FindElement(By.Id("email")).SendKeys(String.Empty);
            _driver.FindElement(By.Id("password")).SendKeys("password");
            _driver.FindElement(By.Id("enter")).Click();
            Assert.Contains("Это поле обязательно для заполнения", _driver.PageSource);
        }
    }
}