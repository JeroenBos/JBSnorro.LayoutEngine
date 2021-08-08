using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.IO;
using System;

public static class LayoutEngine
{
	/// <summary>
	/// Opens the index.html page in the specified directory for consumption by a <see cref="IMeasurer{T}"/>.
	/// </summary>
	public static RemoteWebDriver OpenDir(string dir)
	{
		return OpenPage(Path.Combine(dir, "index.html"));
	}
	/// <summary>
	/// Opens the website at the specified path for consumption by a <see cref="IMeasurer{T}"/>.
	/// </summary>
	public static RemoteWebDriver OpenPage(string path)
	{
		var filePath = Path.GetFullPath(path);
		if (filePath.StartsWith("file://"))
			throw new ArgumentException($"{nameof(path)} shouldn't start with 'file://'");
		if (!File.Exists(filePath))
			throw new ArgumentException($"The path does not exist: '{path}'", "--dir");

		filePath = filePath.ToFileSystemPath();
		var options = new ChromeOptions();
		options.AddArgument("--headless");
		options.AddArgument("--disable-gpu");
		options.AddArgument("--allow-file-access-from-files");


		var driver = new ChromeDriver(options);
		driver.Navigate().GoToUrl(filePath);
		return driver;
	}

	private static string ToFileSystemPath(this string path)
	{
		if (!path.StartsWith("file:"))
			path = "file:///" + path;
		return path;
	}
}
