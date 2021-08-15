using System.IO;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using JBSnorro;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// Tests the publication artifacts.
/// </summary>
public class PublicationTests
{
	public static string CurrentPath => Directory.GetCurrentDirectory();
	public static string RepositoryRoot
	{
		get
		{
			for (DirectoryInfo? directory = new DirectoryInfo(CurrentPath);
				 directory != null;
				 directory = directory.Parent)
			{
				if (File.Exists(Path.Combine(directory.FullName, "LayoutEngine.sln")))
					return directory.FullName;
			}
			throw new FileNotFoundException("LayoutEngine.sln");
		}
	}
	private static string ArtifactPath
	{
		get => Path.Combine(RepositoryRoot, "LayoutEngine", "publish", ArtifactFileName);
	}
	private static string ArtifactFileName
	{
		get => OperatingSystem.IsWindows() ? "LayoutEngine.exe" : "LayoutEngine";
	}

	[Test]
	public async Task Test_That_The_Extracted_Driver_Is_Resolved()
	{
		var executablePath = Path.Combine(JBSnorro.Extensions.CreateTemporaryDirectory(), ArtifactFileName);
		File.Copy(ArtifactPath, executablePath, overwrite: true);
		Assert.IsTrue(File.Exists(executablePath), "File doesn't exist");

		var htmlPathArg = Path.GetFullPath(Path.Combine(CurrentPath, "Index.html")).WrapInDoubleQuotes();
		var result = await ProcessExtensions.WaitForExitAndReadOutputAsync(executablePath, "--file", htmlPathArg);

		Assert.AreEqual(result.ExitCode, 0);
		Assert.IsTrue(result.StandardOutput.EndsWith("STYLE,0,0,0,0\n"));
	}
}
