using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using static JBSnorro.Extensions;

namespace JBSnorro.Web
{
	public static class LayoutEngine
	{
		/// <summary>
		/// Opens the index.html page in the specified directory for consumption by a <see cref="IMeasurer{T}"/>.
		/// </summary>
		public static RemoteWebDriver OpenDir(string dir)
		{
			if (dir == null)
				throw new ArgumentNullException(nameof(dir));
			if (!Directory.Exists(dir))
				if (File.Exists(dir))
					throw new ArgumentException($"The path is a file, not a directory: '{dir}'", "--dir");
				else
					throw new ArgumentException($"The directory does not exist: '{dir}'", "--dir");


			string filePath1 = Path.GetFullPath(Path.Combine(dir, "index.html"));
			string filePath2 = Path.GetFullPath(Path.Combine(dir, "Index.html"));
			bool file1Exists = File.Exists(filePath1);
			bool file2Exists = !file1Exists && File.Exists(filePath2);
			if (!file1Exists && !file2Exists)
			{
				throw new ArgumentException($"No 'index.html' or 'Index.html' file found in dir '{dir}'");
			}
			return OpenPage(file1Exists ? filePath1 : filePath2);
		}
		/// <summary>
		/// Opens the website at the specified path for consumption by a <see cref="IMeasurer{T}"/>.
		/// </summary>
		public static RemoteWebDriver OpenPage(string fullPath, bool showHead = false)
		{
			if (fullPath == null)
				throw new ArgumentNullException(nameof(fullPath));
			if (fullPath.StartsWith("file://"))
				throw new ArgumentException($"{nameof(fullPath)} shouldn't start with 'file://'", nameof(fullPath));
			if (!IsFullPath(fullPath))
				throw new ArgumentException($"'{fullPath}' is not a full path", nameof(fullPath));
			if (!File.Exists(fullPath))
				throw new ArgumentException($"The file does not exist: '{fullPath}'", nameof(fullPath));

			var driver = CreateDriver(headless: !showHead); // don't dispose; it's returned
			System.Diagnostics.Trace.WriteLine($"Opening file '{fullPath.ToFileSystemPath()}'");
			driver.Navigate().GoToUrl(fullPath.ToFileSystemPath());
			return driver;
		}
		private static ChromeDriver CreateDriver(bool headless)
		{
			// create service before creating ChromeOptions due to its static ctor crashing otherwise. Don't dispose; is returned
			var service = CreateDriverService();
			var options = new ChromeOptions();
			if (headless)
			{
				options.AddArgument("--headless");
			}
			options.AddArgument("--disable-gpu");
			options.AddArgument("--allow-file-access-from-files");

			var driver = new ChromeDriver(service, options);
			driver.AssertBrowserAndDriverVersionsCompatible();
			return driver;
		}
		private static ChromeDriverService CreateDriverService()
		{
			// The default services searches in the directory of the executing binary, and PATH.
			// But, the published artifact is extracted somewhere and run there, which is where Selenium searches
			// Here we specify to find it next to the artifact instead
			return ChromeDriverService.CreateDefaultService(Directory.GetCurrentDirectory());
		}
		/// <summary>
		/// Gets all the bounding client rectangles of the html elements in the specified driver by element xpath.
		/// </summary>
		public static IReadOnlyDictionary<string, TaggedRectangle> MeasureBoundingClientsRects(RemoteWebDriver driver)
		{
			return new BoundingRectMeasurer().Measure(driver);
		}
		/// <summary>
		/// Gets all the bounding client rectangles of the html elements in the specified driver order by element xpath.
		/// </summary>
		public static IEnumerable<TaggedRectangle> GetSortedMeasuredBoundingClientsRects(RemoteWebDriver driver)
		{
			return MeasureBoundingClientsRects(driver)
					  .OrderBy(pair => pair.Key)
					  .Select(pair => pair.Value);
		}

		public static void AssertBrowserAndDriverVersionsCompatible(this ChromeDriver driver)
		{
			string warningReason;


			var browserVersion = driver.Capabilities.GetCapability("browserVersion") as string;
			if (browserVersion != null)
			{
				var browser = driver.Capabilities.GetCapability("chrome") as Dictionary<string, object>;
				if (browser != null)
				{
					if (browser.TryGetValue("chromedriverVersion", out object? driverVersionObj))
					{
						if (driverVersionObj is string { Length: >= 3 } driverVersion)
						{
							if (browserVersion == driverVersion[..browserVersion.Length])
							{
								return; // OK. versions are identical
							}
							if (browserVersion[..2] == driverVersion[..2])
							{
								Console.WriteLine($"Warning: browser minor version and driver minor version differ: {browserVersion} vs {driverVersion[..browserVersion.Length]}");
								return; // OK, major versions are identical
							}
							else
							{
								string error = $"The browser and driver versions aren't compatible: {browserVersion} vs {driverVersion}";
								Console.WriteLine(error);
								throw new InvalidOperationException(error);
							}
						}
						else
						{
							warningReason = "capability 'chrome.chromedriverVersion' is not a string";
						}
					}
					else
					{
						warningReason = "capability 'chrome.chromedriverVersion' not found";
					}
				}
				else
				{
					warningReason = "capability 'chrome' not found.";
				}
			}
			else
			{
				warningReason = "capability 'browserVersion' not found.";
			}

			Console.WriteLine("Warning: browser version and driver version compatibility could not be determined: " + warningReason);
		}
	}
}
