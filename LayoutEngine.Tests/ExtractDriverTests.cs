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
	public void Test_Extract_Driver()
	{
		string extension = OperatingSystem.IsWindows() ? ".exe" : "";
		var dir = JBSnorro.Extensions.CreateTemporaryDirectory();

		Program.EnsureDriverExtracted(dir);
		string path = Path.Combine(dir, $"chromedriver{extension}");
		Assert.IsTrue(File.Exists(path));

		int expectedHashCode = OperatingSystem.IsWindows() ? 847548445 : 1081775203;
		// the following does not work in CI, because the path has 1 extra depth (the runtime identifier):
		// int expectedHashCode = File.ReadAllBytes($"../../../../LayoutEngine/chromedriver{extension}").ComputeHashCode();

		Assert.AreEqual(expectedHashCode, File.ReadAllBytes(path).ComputeHashCode());
	}
}
