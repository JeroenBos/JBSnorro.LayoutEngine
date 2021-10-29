using JBSnorro.Web;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using System.IO;

static class Extensions
{
	/// <summary>
	/// When running as CI, configures the options to run headlessly.
	/// </summary>
	public static void AsHeadlessInCI(this ChromeOptions options)
	{
#if CI
		options.AddArgument("--headless");
		options.AddArgument("--disable-gpu");
#endif
	}
}


public class DemonstrateChromedriverBug
{
	[Test]
	[TestCase(false)]
#if !CI
    // Doesn't work in GitHub actions, but running the test locally should still run fine
	[TestCase(true)]
#endif
	public void Headless_And_Headful_Chromedrivers_Round_Element_Height_Differently(bool showHead)
	{
		// Arrange
		using var driver = LayoutEngine.OpenPage(Path.GetFullPath("OneElementWithFractionalHeight.html"), showHead);
		var measurer = new BoundingRectMeasurer();

		// Act
		var sizesByXPath = measurer.Measure(driver)!;

		// Assert
		const string spanWithClassKatex_HTMLXPath = "/HTML[1]/BODY[1]/SPAN[1]"; // class=katex-html
		Assert.IsTrue(sizesByXPath.ContainsKey(spanWithClassKatex_HTMLXPath));

		TaggedRectangle divSizes = sizesByXPath[spanWithClassKatex_HTMLXPath];

		Assert.AreEqual(divSizes.Tagname, "SPAN");
		Assert.AreEqual(divSizes.X, 0);
		Assert.AreEqual(divSizes.Y, 0);

		if (showHead)
		{
			Assert.AreEqual(divSizes.Width, 25.71875f);
			Assert.AreEqual(divSizes.Height, 17.333334f);
		}
		else
		{
			Assert.AreEqual(divSizes.Width, 25.703125f);
			Assert.AreEqual(divSizes.Height, 17f);
		}
	}

}