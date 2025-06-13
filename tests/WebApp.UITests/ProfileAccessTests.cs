using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace WebApp.UITests
{
    [TestFixture]
    public class ProfileAccessTests : BaseUITest
    {
        private string _username = "";
        private const string Password = "Tests123!";

        [Test]
        public void Authenticated_User_Sees_Profile_Page()
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));

            // 1) Register
            _username = "ui" + DateTime.Now.Ticks;
            Driver.Navigate().GoToUrl(AppUrl + "/register");
            wait.Until(d => d.FindElements(By.CssSelector("input.form-control")).Count >= 3);
            var regInputs = Driver.FindElements(By.CssSelector("input.form-control"));
            regInputs[0].SendKeys(_username);
            regInputs[1].SendKeys($"{_username}@example.com");
            regInputs[2].SendKeys(Password);
            Driver.FindElement(By.CssSelector("button.btn.btn-primary")).Click();

            // 2) Login
            wait.Until(d => d.Url.Contains("/login"));
            var loginInputs = Driver.FindElements(By.CssSelector("input.form-control"));
            loginInputs[0].SendKeys(_username);
            loginInputs[1].SendKeys(Password);
            Driver.FindElement(By.CssSelector("button.btn.btn-primary")).Click();

            // 3) Wait for client‐side redirect into /homeuser
            wait.Until(d => d.Url.EndsWith("/homeuser", StringComparison.OrdinalIgnoreCase));

            // 4) Click the “Perfil” button in the top row
            var perfilBtn = wait.Until(d =>
                d.FindElement(By.CssSelector("a.btn-outline-primary[href='/profile']")));
            perfilBtn.Click();

            // 5) Wait for the profile heading
            wait.Until(d =>
                d.FindElement(By.TagName("h3")).Text.Contains("Perfil do Utilizador"));

            // 6) Assert
            var heading = Driver.FindElement(By.TagName("h3")).Text;
            Assert.AreEqual("Perfil do Utilizador", heading);
        }
    }
}
