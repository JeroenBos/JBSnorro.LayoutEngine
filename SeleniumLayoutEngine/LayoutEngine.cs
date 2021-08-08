using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

		var options = new ChromeOptions();
		options.AddArgument("--headless");
		options.AddArgument("--disable-gpu");
		options.AddArgument("--allow-file-access-from-files");
#if CI
		options.AddArgument("--whitelisted-ips=\"\"");
#endif


		var driver = new ChromeDriver(options);
		System.Diagnostics.Trace.WriteLine($"Opening file '{fullPath}'");
		driver.Navigate().GoToUrl(fullPath);
		return driver;
	}
	/// <summary>
	/// Gets all the bounding client rectangles of the html elements in the specified driver by element xpath.
	/// </summary>
	public static IReadOnlyDictionary<string, RectangleF> MeasureBoundingClientsRects(RemoteWebDriver driver)
	{
		return new BoundingRectMeasurer().Measure(driver);
	}
	/// <summary>
	/// Gets all the bounding client rectangles of the html elements in the specified driver order by element xpath.
	/// </summary>
	public static IEnumerable<RectangleF> GetSortedMeasuredBoundingClientsRects(RemoteWebDriver driver)
	{
		return MeasureBoundingClientsRects(driver)
				  .OrderBy(pair => pair.Key)
				  .Select(pair => pair.Value);
	}

	/// <summary> Gets whether the path is a full path in the current OS. </summary>
	/// <see href="https://stackoverflow.com/a/35046453/308451" />
	public static bool IsFullPath(string path)
	{
		if (OperatingSystem.IsWindows())
			return IsFullPathInWindows(path);
		else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
			return IsFullPathInUnix(path);

		throw new NotImplementedException("IsFullPath not implemented yet for current OS");
	}
	/// <summary> Gets whether the path is a full path. </summary>
	/// <see href="https://stackoverflow.com/a/2202096/308451"/>
	public static bool IsFullPathInUnix(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return false;

		// check if valid linux path:
		if (path.Contains((char)0))
			return false;

		// char 47 is '/', so we can skip checking it
		if (path.StartsWith("/"))
			return true;

		return false;
	}
	/// <summary> Gets whether the path is a full path. </summary>
	/// <see href="https://stackoverflow.com/a/35046453/308451" />
	public static bool IsFullPathInWindows(string path)
	{
		return !string.IsNullOrWhiteSpace(path)
			&& path.IndexOfAny(Path.GetInvalidPathChars().ToArray()) == -1
			&& Path.IsPathRooted(path)
			&& !(Path.GetPathRoot(path)?.Equals("\\", StringComparison.Ordinal) ?? false);
	}
}
