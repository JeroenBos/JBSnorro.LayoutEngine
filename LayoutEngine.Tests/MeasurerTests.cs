using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using JBSnorro.Web;
using LayoutEngine = JBSnorro.Web.LayoutEngine;

public class MeasurerTests
{
    class DummyMeasurer : IMeasurer<object>
    {
        object IMeasurer<object>.Measure(IWebElement element, RemoteWebDriver driver) => element;
    }
    [Test]
    public void BodylessHtmlDoesntRaiseException()
    {
        // Arrange
        using var driver = LayoutEngine.OpenPage(Path.GetFullPath("Bodyless.html"));
        IMeasurer<object> measurer = new DummyMeasurer();

        // Act
        IWebElement body = (IWebElement)measurer.Measure(driver);

        // Assert
        Assert.IsNotNull(body);
        Assert.AreEqual("body", body!.TagName);
    }
}

public class BoundingRectsMeasurerTests
{
    [Test]
    public void Explicit_Sizes_Can_Be_Read_From_Div()
    {
        // Arrange
        using var driver = LayoutEngine.OpenPage(Path.GetFullPath("OneElementWithSizes.html"));
        var measurer = new BoundingRectMeasurer();

        // Act
        var sizesByXPath = measurer.Measure(driver)!;

        // Assert
        const string divXPath = "/HTML[1]/BODY[1]/DIV[1]";
        Assert.IsTrue(sizesByXPath.ContainsKey(divXPath));

        TaggedRectangle divSizes = sizesByXPath[divXPath];
        Assert.AreEqual(divSizes, new TaggedRectangle("div", 8, 8, 400.296875f, 300.5f));
    }
}

public class TaggedRectangleFormattingTests
{
    [Test]
    public void TestTaggedRectangleormatter()
    {
        var rect = new TaggedRectangle("div", 1, 0, 1 / 3f, 0.2f);

        var formatted = rect.Format();

        Assert.AreEqual("div,1,0,0.33333334,0.2", formatted);
    }
}


public class DemonstrateChromedriverBug
{
    [Test]
    [TestCase(false)]
#if CI
    [TestCase(true)]
#endif
    public void Headless_And_Headfull_Chromedrivers_Round_Element_Height_Differently(bool showHead)
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