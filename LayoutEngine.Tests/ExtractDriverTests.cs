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

		int expectedHashCode = File.ReadAllBytes($"../../../../LayoutEngine/chromedriver{extension}").ComputeHashCode();
		Assert.AreEqual(expectedHashCode, File.ReadAllBytes(path).ComputeHashCode());

	}
}
