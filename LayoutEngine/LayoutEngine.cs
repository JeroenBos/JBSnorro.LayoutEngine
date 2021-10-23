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
		public static RemoteWebDriver OpenPage(string fullPath)
		{
			if (fullPath == null)
				throw new ArgumentNullException(nameof(fullPath));
			if (fullPath.StartsWith("file://"))
				throw new ArgumentException($"{nameof(fullPath)} shouldn't start with 'file://'", nameof(fullPath));
			if (!IsFullPath(fullPath))
				throw new ArgumentException($"'{fullPath}' is not a full path", nameof(fullPath));
			if (!File.Exists(fullPath))
				throw new ArgumentException($"The file does not exist: '{fullPath}'", nameof(fullPath));

			var service = CreateDriverService(); // call before creating ChromeOptions due to its static ctor crashing otherwise
			var options = new ChromeOptions();
			options.AddArgument("--headless");
			options.AddArgument("--disable-gpu");
			options.AddArgument("--allow-file-access-from-files");
			options.AddArgument("--disable-dev-shm-usage");

			var driver = new ChromeDriver(service, options);
			System.Diagnostics.Trace.WriteLine($"Opening file '{fullPath.ToFileSystemPath()}'");
			driver.Navigate().GoToUrl(fullPath.ToFileSystemPath());
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
	}
}
