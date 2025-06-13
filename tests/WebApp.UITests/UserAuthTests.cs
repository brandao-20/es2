using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace WebApp.UITests
{
    [TestFixture]
    public class UserAuthTests : BaseUITest
    {
        [Test]
        public void Register_And_Login_Flow_Works()
        {
            Driver.Navigate().GoToUrl(AppUrl + "/register");

            // Aguarda o formulário de registo
            new WebDriverWait(Driver, TimeSpan.FromSeconds(15))
                .Until(d => d.FindElements(By.CssSelector("input.form-control")).Count >= 3);

            // Preenche o formulário de registo
            var inputs = Driver.FindElements(By.CssSelector("input.form-control"));
            string uniqueUsername = "tests" + DateTime.Now.Ticks;
            inputs[0].SendKeys(uniqueUsername);          // Username
            inputs[1].SendKeys(uniqueUsername + "@example.com"); // Email
            inputs[2].SendKeys("Tests123!");            // Password
            Driver.FindElement(By.CssSelector("button.btn.btn-primary")).Click();

            // Aguarda redirecionamento após registo
            new WebDriverWait(Driver, TimeSpan.FromSeconds(15))
                .Until(d => d.Url.Contains("/login"));

            // Faz login
            Driver.Navigate().GoToUrl(AppUrl + "/login");
            new WebDriverWait(Driver, TimeSpan.FromSeconds(15))
                .Until(d => d.FindElements(By.CssSelector("input.form-control")).Count >= 2);

            inputs = Driver.FindElements(By.CssSelector("input.form-control"));
            inputs[0].SendKeys(uniqueUsername);
            inputs[1].SendKeys("Tests123!");
            Driver.FindElement(By.CssSelector("button.btn.btn-primary")).Click();

            // Aguarda o redirecionamento após login
            new WebDriverWait(Driver, TimeSpan.FromSeconds(15))
                .Until(d => d.FindElement(By.TagName("h1")).Text.Contains("Bem-vindo"));

            Assert.IsTrue(Driver.FindElement(By.TagName("h1")).Text.Contains("Bem-vindo"), "Login deve ser bem-sucedido.");
        }
    }
}
