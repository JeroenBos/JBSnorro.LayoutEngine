using System.IO;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using JBSnorro;
using JBSnorro.Web;
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
	private static ProcessStartInfo ArtifactFixture()
    {
		var executablePath = Path.Combine(JBSnorro.Extensions.CreateTemporaryDirectory(), ArtifactFileName);
		File.Copy(ArtifactPath, executablePath, overwrite: true);
		Assert.IsTrue(File.Exists(executablePath), "File doesn't exist");

		var processStart = new ProcessStartInfo(executablePath)
		{
			WorkingDirectory = Path.GetDirectoryName(executablePath),
		};
		return processStart;
	}

	[Test]
	public async Task Test_That_The_Extracted_Driver_Is_Resolved()
	{
		// Arrange
		var executable = ArtifactFixture();
		var htmlPathArg = Path.GetFullPath(Path.Combine(CurrentPath, "Index.html")).WrapInDoubleQuotes();
		executable.Arguments += "--file " + htmlPathArg;

		var result = await ProcessExtensions.WaitForExitAndReadOutputAsync(executable, timeout: 10_000);

		Assert.AreEqual(0, result.ExitCode, result.ErrorOutput);
		Assert.IsTrue(result.StandardOutput.EndsWith("STYLE,0,0,0,0\n"));
	}
}
