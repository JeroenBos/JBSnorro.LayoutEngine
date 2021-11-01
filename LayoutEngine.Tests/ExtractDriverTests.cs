using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JBSnorro;
using JBSnorro.Web;
using NUnit.Framework;

public class ExtractDriverTests
{
	[Test]
	public async Task Test_Extract_Driver_Is_Same_As_In_LayoutEngine_Folder()
	{
		var expected = GetExpectedHashcode();
		string extension = OperatingSystem.IsWindows() ? ".exe" : "";
		var dir = JBSnorro.Extensions.CreateTemporaryDirectory();

		await Program.EnsureDriverExtracted(dir);
		string path = Path.Combine(dir, $"chromedriver{extension}");
		Assert.IsTrue(File.Exists(path));

		unchecked
		{
			nuint expectedHashCode = await expected;

			Assert.AreEqual(expectedHashCode, path.ComputeFileHash());
		}
	}
	static Task<nuint> GetExpectedHashcode(bool? windows = null)
	{
		string extension = (windows ?? OperatingSystem.IsWindows()) ? ".exe" : "";

		string path = Path.Combine(Properties.RepoRoot, "LayoutEngine", "chromedriver" + extension);

		return path.ComputeFileHashAsync();
	}

	public async Task HelperToExtractGuidFromDifferentOS()
	{
		const string extension = ".exe";
		var dir = JBSnorro.Extensions.CreateTemporaryDirectory();
		await Program.EnsureDriverExtracted(extension, dir: dir);

		string path = Path.Combine(dir, "chromedriver" + extension);
		Console.WriteLine(path.ComputeFileHash());
	}
}
