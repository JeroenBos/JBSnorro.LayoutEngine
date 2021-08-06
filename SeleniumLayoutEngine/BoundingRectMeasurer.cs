using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Reflection.Metadata;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using static System.Net.WebRequestMethods;
using System;
using OpenQA.Selenium.Remote;

/// <summary>
/// Obtains the <see cref="RectangleF"/> boundingClientRectangle of each <see cref="IWebElement"/>.
/// </summary>
public class BoundingRectMeasurer : Measurer<IReadOnlyDictionary<string, RectangleF>>
{
	protected override IReadOnlyDictionary<string, RectangleF> Measure(IWebElement element, RemoteWebDriver driver)
	{
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
