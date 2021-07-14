using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace WebStudio.Tests
{
    public class BasicSteps
    {
        private readonly IWebDriver _driver;
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
    }
}