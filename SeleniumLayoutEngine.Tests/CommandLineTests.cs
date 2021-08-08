using System.IO;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading.Tasks;
using System;

namespace SeleniumLayoutEngine.Tests
{
	public class CommandLineTests
	{
		[Test]
		public async Task Open_Index()
		{
			using (new CaptureStdOut(out TextWriter stdOut, out TextWriter stdErr))
			{
				await Program.Main(new string[] { "--dir", "." });
				string? output = stdOut.ToString();

				string expected = @"0,0,800,600
0,0,800,600
0,0,800,0
0,0,0,0
0,0,0,0
0,0,0,0
0,0,0,0
".Replace("\r", "");
				Assert.AreEqual("", stdErr.ToString());
				Assert.AreEqual(expected, output);
			}
		}
		[Test]
		public async Task Open_One_Element_With_Sizes_Print_The_Size()
		{
			using (new CaptureStdOut(out TextWriter stdOut, out TextWriter stdErr))
			{
				await Program.Main(new string[] { "--file", "OneElementWithSizes.html" });

				// Assert.AreEqual("", stdErr.ToString());

				string? output = stdOut.ToString();
				string expected = @"0,0,800,316.5
8,8,784,300.5
8,8,400.29688,300.5
0,0,0,0
".Replace("\r", "");
				Assert.AreEqual(expected, output);
			}
		}

	}
}

class CaptureStdOut : IDisposable
{
	private readonly TextWriter? stdOut;
	private readonly TextWriter? stdErr;

	public CaptureStdOut(out TextWriter stdOut)
	{
		stdOut = new StringWriter();
		if (stdOut != null)
		{
			this.stdOut = Console.Out;
			Console.SetOut(stdOut);
		}
	}
	public CaptureStdOut(out TextWriter stdOut, out TextWriter stdErr) : this(out stdOut)
	{
		stdErr = new StringWriter();
		if (stdErr != null)
		{
			this.stdErr = Console.Error;
			Console.SetError(stdErr);
		}
	}
	public void Dispose()
	{
		if (this.stdOut != null)
		{
			Console.SetError(stdOut);
			this.stdOut.Dispose();
		}
		if (this.stdErr != null)
		{
			Console.SetError(stdErr);
			this.stdErr.Dispose();
		}

	}
}