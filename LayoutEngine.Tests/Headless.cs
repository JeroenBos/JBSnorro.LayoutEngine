using JBSnorro.Web;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

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


#if !CI
    // Doesn't work in GitHub actions, but running the test locally should still run fine
	[Test]
#endif
	public async Task Open_One_Element_With_Sizes_Print_The_Size_Headful()
	{
		CaptureStdOut output;
		using (output = new CaptureStdOut())
		{
			await Program.Main(new string[] { "--file", "OneElementWithSizes.html", "--headful" });
		}

		Assert.AreEqual("", output.StdErr);
		string expected = @"########## RECTANGLES INCOMING (V1) ##########
HTML,0,0,1906,316.5
BODY,8,8,1890,300.5
DIV,8,8,400.2917,300.5
HEAD,0,0,0,0
".Replace("\r", "");
		string stdOut = output.StdOut!.SkipCIConnectionFailedLines();
		if (expected != stdOut)
		{
			Console.WriteLine("output.stdOut");
			Console.WriteLine(stdOut);
			Console.WriteLine(stdOut.StartsWith("Connection refused [::ffff:127.0.0.1]:"));
		}
		Assert.AreEqual(expected, stdOut);
	}

	[Test]
	public async Task CacheIsDifferentForHeadful()
{
		const string file = "OneElementWithSizes.html";
		const string cachePath = ".layoutenginecache/";

		var headlessHash = await new Cache(headless: true).TryGetValue(file, dir: null, cachePath);
		var headfulHash = await new Cache(headless: false).TryGetValue(file, dir: null, cachePath);

		Assert.AreNotEqual(headfulHash, headlessHash);
	}
}
