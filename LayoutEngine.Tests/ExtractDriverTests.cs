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
	public async Task Test_Extract_Driver()
	{
		string extension = OperatingSystem.IsWindows() ? ".exe" : "";
		var dir = JBSnorro.Extensions.CreateTemporaryDirectory();

		await Program.EnsureDriverExtracted(dir);
		string path = Path.Combine(dir, $"chromedriver{extension}");
		Assert.IsTrue(File.Exists(path));

		unchecked
		{
			nuint expectedHashCode = OperatingSystem.IsWindows() ? (nuint)0x4ff61c231f22aab3 : (nuint)6311131775771370345;
			// the following does not work in CI, because the path has 1 extra depth (the runtime identifier):
			// int expectedHashCode = $"../../../../LayoutEngine/chromedriver{extension}".ComputeFileHashCode();

			Assert.AreEqual(expectedHashCode, path.ComputeFileHash());
		}
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
