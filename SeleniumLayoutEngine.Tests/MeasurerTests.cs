using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Drawing;

namespace SeleniumLayoutEngine.Tests
{
	public class MeasurerTests
	{
		class DummyMeasurer : Measurer<object>
		{
			protected override IWebElement Measure(IWebElement element, RemoteWebDriver _) => element;
		}
		[Test]
		public void BodylessHtmlDoesntRaiseException()
		{
			var measurer = new DummyMeasurer();
			IWebElement? body = (IWebElement?)measurer.Measure("bodyless.html");

			Assert.IsNotNull(body);
			Assert.Throws<WebDriverException>(() => { var _ = body!.TagName; });
		}
	}

	public class BoundingRectsMeasurerTests
	{
		[Test]
		public void BodylessHtmlDoesntRaiseException()
		{
			var measurer = new BoundingRectMeasurer();
			var sizesByXPath = measurer.Measure("OneElementWithSizes.html")!;

			const string divXPath = "/HTML[1]/BODY[1]/DIV[1]";
			Assert.IsTrue(sizesByXPath.ContainsKey(divXPath));

			RectangleF divSizes = sizesByXPath[divXPath];
			Assert.AreEqual(divSizes, new RectangleF(8, 8, 400.296875f, 300.5f));
		}
	}
}
