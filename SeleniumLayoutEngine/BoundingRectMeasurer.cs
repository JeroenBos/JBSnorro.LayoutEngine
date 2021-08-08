using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Reflection.Metadata;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using OpenQA.Selenium.Remote;
using System.Globalization;

/// Obtains the <see cref="RectangleF"/> boundingClientRectangle of each <see cref="IWebElement"/>.
/// </summary>
public class BoundingRectMeasurer : IMeasurer<IReadOnlyDictionary<string, RectangleF>>
{
	IReadOnlyDictionary<string, RectangleF> IMeasurer<IReadOnlyDictionary<string, RectangleF>>.Measure(IWebElement element, RemoteWebDriver driver)
	{
		if (element.TagName != "body")
			throw new Exception("Expected html body element to have tag 'body'");

		string jsFunctionName = "getBoundingClientRect";
		string jsFunction = $"function {jsFunctionName}(element) {{ return element.getBoundingClientRect(); }}";
		RectangleF converter(object boundingRectReturnValue)
		{
			var boundingRect = (IReadOnlyDictionary<string, object>)boundingRectReturnValue;
			return new RectangleF(
				Convert.ToSingle(boundingRect["x"]),
				Convert.ToSingle(boundingRect["y"]),
				Convert.ToSingle(boundingRect["width"]),
				Convert.ToSingle(boundingRect["height"])
			);
		}

		var result = driver.ForeachXPaths(jsFunction, jsFunctionName, converter);
		return result;
	}
}
public static class BoundingRectMeasurerExtensions
{
	/// <inheritdoc cref="IMeasurer{T}.Measure(IWebElement, RemoteWebDriver)"/>
	/// <remarks> This method is implemented as extension method instead of instance method to allow for calling the base default interface method
	/// and to ease the developer's life by allowing to call the default interface method without casting. </remarks>
	public static IReadOnlyDictionary<string, RectangleF> Measure(this BoundingRectMeasurer measurer, RemoteWebDriver driver)
	{
		return ((IMeasurer<IReadOnlyDictionary<string, RectangleF>>)measurer).Measure(driver);
	}

	/// <summary>
	/// Formats the rectangle into 4 comma-separated numbers, with the period as decimal point.
	/// </summary>
	public static string Format(this RectangleF rectangle)
	{
		return string.Join(',', new float[] { rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height }
					 .Select(f => f.ToString(CultureInfo.InvariantCulture)));
	}
}
