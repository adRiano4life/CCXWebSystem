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
    }
}