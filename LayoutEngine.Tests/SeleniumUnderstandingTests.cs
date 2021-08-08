using System.IO;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;

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
		chromeDriver.Navigate().GoToUrl(Path.Combine(CurrentPath, "Index.html").ToFileSystemPath());
		IWebElement element = chromeDriver.FindElement(By.Id("test"));

		Assert.IsNotNull(element);
	}

	[Test]
	public void TestResolveAccessToLocalCSS()
	{
		var options = new ChromeOptions();
		options.AsHeadlessInCI();
		options.AddArgument("--allow-file-access-from-files");
		IWebDriver chromeDriver = new ChromeDriver(options);
		chromeDriver.Navigate().GoToUrl(Path.Combine(CurrentPath, "Index.html").ToFileSystemPath());

		// The KaTeX_Main is listed in blatex.css under .katex, which the element with id "test" has
		IWebElement element = chromeDriver.FindElement(By.Id("test"));
		Assert.IsTrue(element.GetCssValue("font").Contains("KaTeX_Main"));
	}
}