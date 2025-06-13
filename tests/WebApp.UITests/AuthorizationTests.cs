using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace WebApp.UITests
{
    [TestFixture]
    public class AuthorizationTests : BaseUITest
    {
        [Test]
        public void Unauthorized_User_Can_Access_Comparison_Page_Defaults_To_Prompt()
        {
            // Navega para a página de comparação sem autenticação
            Driver.Navigate().GoToUrl(AppUrl + "/comparar-precos");

            // Aguarda até o dropdown de produtos estar presente
            new WebDriverWait(Driver, TimeSpan.FromSeconds(15))
                .Until(d => d.FindElement(By.CssSelector("select.form-control")).Displayed);

            // Verifica se o dropdown padrão "Escolha um produto..." está presente
            var defaultOption = Driver.FindElement(By.CssSelector("select.form-control > option[value='0']"));
            Assert.IsTrue(defaultOption.Text.Contains("Escolha um produto"), 
                "Página de comparação deve exibir a opção padrão para produto.");

            // Garante que exista pelo menos essa opção
            var optionsCount = Driver.FindElements(By.CssSelector("select.form-control option")).Count;
            Assert.IsTrue(optionsCount >= 1, 
                "Deve haver ao menos a opção padrão no dropdown.");
        }
    }
}
