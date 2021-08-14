﻿using System;
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

		nuint expectedHashCode = OperatingSystem.IsWindows() ? (nuint)9178564069832298525 : (nuint)0xe2f4c80a407a9463;
		// the following does not work in CI, because the path has 1 extra depth (the runtime identifier):
		// int expectedHashCode = $"../../../../LayoutEngine/chromedriver{extension}".ComputeFileHashCode();

		Assert.AreEqual(expectedHashCode, path.ComputeFileHash());
	}
}
