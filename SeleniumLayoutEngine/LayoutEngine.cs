﻿using OpenQA.Selenium.Chrome;
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
			throw new ArgumentException("Not a full path", nameof(fullPath));
		if (!File.Exists(fullPath))
			throw new ArgumentException($"The file does not exist: '{fullPath}'", nameof(fullPath));

		var options = new ChromeOptions();
		options.AddArgument("--headless");
		options.AddArgument("--disable-gpu");
		options.AddArgument("--allow-file-access-from-files");


		var driver = new ChromeDriver(options);
		driver.Navigate().GoToUrl(fullPath.ToFileSystemPath());
		return driver;
	}

	private static string ToFileSystemPath(this string path)
	{
		if (!path.StartsWith("file:"))
			path = "file:///" + path;
		return path;
	}
	/// <summary> Gets whether the path is a full path. </summary>
	/// <see href="https://stackoverflow.com/a/35046453/308451"/>
	public static bool IsFullPath(string path)
	{
		return !string.IsNullOrWhiteSpace(path)
			&& path.IndexOfAny(Path.GetInvalidPathChars().ToArray()) == -1
			&& Path.IsPathRooted(path)
			&& !Path.GetPathRoot(path)!.Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
	}
}