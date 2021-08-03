using System.IO;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;

namespace SeleniumLayoutEngine.Tests
{
	public class SeleniumUnderstandingTests
	{
		public static string CurrentPath => Directory.GetCurrentDirectory();

		[Test]
		public void TestOpenLocalIndexPage()
		{
			var options = new ChromeOptions();
			options.AsHeadlessInCI();
			options.AddArgument("--allow-file-access-from-files");
			IWebDriver chromeDriver = new ChromeDriver(options);
			chromeDriver.Navigate().GoToUrl("file:///" + Path.Combine(CurrentPath, "Index.html"));
			IWebElement element = chromeDriver.FindElement(By.Id("test"));

			Assert.NotNull(element);
		}

		[Test]
		public void TestResolveAccessToLocalCSS()
		{
			var options = new ChromeOptions();
			options.AsHeadlessInCI();
			options.AddArgument("--allow-file-access-from-files");
			IWebDriver chromeDriver = new ChromeDriver(options);
			chromeDriver.Navigate().GoToUrl("file:///" + Path.Combine(CurrentPath, "Index.html"));

			// The KaTeX_Main is listed in blatex.css under .katex, which the element with id "test" has
			IWebElement element = chromeDriver.FindElement(By.Id("test"));
			Assert.IsTrue(element.GetCssValue("font").Contains("KaTeX_Main"));
		}
	}
}