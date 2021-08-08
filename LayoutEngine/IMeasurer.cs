using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using static System.Net.WebRequestMethods;

internal interface IMeasurer<out T>
{
	/// <summary>
	/// Measures the sizes of all html elements on the current page on the specified driver.
	/// </summary>
	T Measure(RemoteWebDriver driver)
	{
		IWebElement body = driver.FindElementByXPath("//body");

		if (body.TagName != "body")
			throw new Exception("Expected html body element to have tag 'body'");

		return Measure(body, driver);
	}

	T Measure(IWebElement element, RemoteWebDriver driver);
}
