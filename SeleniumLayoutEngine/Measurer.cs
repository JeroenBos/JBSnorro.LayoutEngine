using System;
using System.IO;
using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

public abstract class Measurer<T> : IMeasurer<T>
{
	/// <inheritdocs/>
	public T? Measure(string path)
	{
		using ChromeDriver driver = CreateDriver(path);

		IWebElement body = driver.FindElementByXPath("//body");

		if (body.TagName != "body")
			throw new Exception("Expected html body element to have tag 'body'");

		return Measure(body, driver);
	}

	private ChromeDriver CreateDriver(string path)
	{
		path = PreparePath(path);

		var options = new ChromeOptions();
		options.AddArgument("--headless");
		options.AddArgument("--disable-gpu");
		options.AddArgument("--allow-file-access-from-files");

		var driver = new ChromeDriver(options);
		driver.Navigate().GoToUrl(path);
		return driver;
	}
	protected abstract T? Measure(IWebElement element, RemoteWebDriver driver);

	private string PreparePath(string path)
	{
		path = Path.GetFullPath(path);
		if (!path.StartsWith("file:"))
			path = "file:///" + path;
		return path;
	}
}
