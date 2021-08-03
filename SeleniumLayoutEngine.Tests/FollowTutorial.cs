using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;

namespace SeleniumLayoutEngine.Tests
{
	public class Tests
	{
		[Test]
		public void TestSeleniumTutorial()
		{
			var options = new ChromeOptions();
			options.AsHeadlessInCI();
			IWebDriver chromeDriver = new ChromeDriver(options);
			chromeDriver.Navigate().GoToUrl("http://automatetheplanet.com/");
			IWebElement element = chromeDriver.FindElement(By.Id("cookie-law-info-bar"));

			Assert.NotNull(element);
		}
	}
}