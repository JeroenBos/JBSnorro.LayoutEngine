#!/usr/bin/env dotnet-script
#r "nuget: NuGet.Configuration, 5.11.0"
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NuGet.Configuration;

// To upgrade cromedriver:
// - manually install the Nuget package to the desired version
// - update this project's version
// - run this script (dotnet script .\UpgradeChromedriver.csx)

Console.ResetColor();
try
{
	Console.WriteLine("Deleting current files");
	string repoRoot = Path.GetDirectoryName(GetCurrentFileName());
	string winDriverDest = Path.Combine(repoRoot, "LayoutEngine", "chromedriver.exe");
	string linuxDriverDest = Path.Combine(repoRoot, "LayoutEngine", "chromedriver");
	File.Delete(winDriverDest);
	File.Delete(linuxDriverDest);
	string winTool = Path.Combine(repoRoot, "LayoutEngine", "publish", "LayoutEngine.exe");
	string linuxTool = Path.Combine(repoRoot, "LayoutEngine", "publish", "LayoutEngine.exe");
	File.Delete(winTool);
	File.Delete(linuxTool);

	// Other things I could do here:
	// - Cancel all processes titled 'chromedriver'.
	// - Delete ./bin and ./obj
	// - Install the same version automatically to Tests.csproj

	Console.WriteLine("Locating drivers");
	var settings = Settings.LoadDefaultSettings(null);
	string webdriverPackage = Path.Join(SettingsUtility.GetGlobalPackagesFolder(settings), "selenium.webdriver.chromedriver");
	if (!Directory.Exists(webdriverPackage))
		throw new Exception("No package directory found");

	string latestPackagePath = Directory.GetDirectories(webdriverPackage)
										.Where(dir => char.IsDigit(new DirectoryInfo(dir).Name[0]))
										.OrderBy(_ => _)
										.LastOrDefault();
	if (latestPackagePath == null)
		throw new Exception("No version found");

	string windriverSrc = Path.Combine(latestPackagePath, "driver", "win32", "chromedriver.exe");
	string linuxdriverSrc = Path.Combine(latestPackagePath, "driver", "linux64", "chromedriver");



	Console.WriteLine("Copying drivers");
	File.Copy(windriverSrc, winDriverDest, overwrite: true);
	File.Copy(linuxdriverSrc, linuxDriverDest, overwrite: true);


	Console.WriteLine("Publishing artifacts");
	var winPublicationProcess = Process.Start(new ProcessStartInfo("dotnet", "publish -c Release -r linux-x64 -o publish")
	{
		WorkingDirectory = Path.Combine(repoRoot, "LayoutEngine")
	});

	var linuxPublicationProcess = Process.Start(new ProcessStartInfo("dotnet", "publish -c Release -r win10-x64 -o publish")
	{
		WorkingDirectory = Path.Combine(repoRoot, "LayoutEngine")
	});

	winPublicationProcess.WaitForExit();
	linuxPublicationProcess.WaitForExit();
	Console.WriteLine("Done");
}
catch (Exception e)
{
	Console.ForegroundColor = ConsoleColor.Red;
	Console.WriteLine(e.Message);
	return 1;
}
string GetCurrentFileName([System.Runtime.CompilerServices.CallerFilePath] string fileName = null)
{
	return fileName;
}