using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection.Metadata;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using OpenQA.Selenium.Remote;
using System.Globalization;
using System.Diagnostics;

namespace JBSnorro.Web
{
	/// Obtains the <see cref="TaggedRectangle"/> boundingClientRectangle of each <see cref="IWebElement"/>.
	/// </summary>
	internal class BoundingRectMeasurer : IMeasurer<IReadOnlyDictionary<string, TaggedRectangle>>
	{
		IReadOnlyDictionary<string, TaggedRectangle> IMeasurer<IReadOnlyDictionary<string, TaggedRectangle>>.Measure(IWebElement element, WebDriver driver)
		{
			if (element.TagName != "body")
				throw new Exception("Expected html body element to have tag 'body'");

			string jsFunctionName = "getBoundingClientRect";
			string jsFunction = $"function {jsFunctionName}(element) {{ var rect = element.getBoundingClientRect(); rect.tagName = element.tagName; return rect; }}";
			TaggedRectangle converter(object boundingRectReturnValue)
			{
				var boundingRect = (IReadOnlyDictionary<string, object>)boundingRectReturnValue;
				return new TaggedRectangle(
					(string)boundingRect["tagName"],
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
	internal static class BoundingRectMeasurerExtensions
	{
		/// <inheritdoc cref="IMeasurer{T}.Measure(IWebElement, RemoteWebDriver)"/>
		/// <remarks> This method is implemented as extension method instead of instance method to allow for calling the base default interface method
		/// and to ease the developer's life by allowing to call the default interface method without casting. </remarks>
		[DebuggerHidden]
		public static IReadOnlyDictionary<string, TaggedRectangle> Measure(this BoundingRectMeasurer measurer, WebDriver driver)
		{
			return ((IMeasurer<IReadOnlyDictionary<string, TaggedRectangle>>)measurer).Measure(driver);
		}
	}
}