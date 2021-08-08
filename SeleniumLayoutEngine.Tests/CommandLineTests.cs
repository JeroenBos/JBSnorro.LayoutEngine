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
			CaptureStdOut output;
			using (output = new CaptureStdOut())
			{
				await Program.Main(new string[] { "--dir", "." });
			}


			// CI debugging statements
			if (!string.IsNullOrEmpty(output.StdErr))
			{
				Console.WriteLine("StdErr:");
				Console.WriteLine(output.StdErr);
			}
			Console.WriteLine("StdOut:");
			Console.WriteLine(output.StdOut);

			Assert.AreEqual("", output.StdErr);
			string expected = @"0,0,800,600
0,0,800,600
0,0,800,0
0,0,0,0
0,0,0,0
0,0,0,0
0,0,0,0
".Replace("\r", "");
			Assert.AreEqual(expected, output.StdOut);
		}
		[Test]
		public async Task Open_One_Element_With_Sizes_Print_The_Size()
		{
			CaptureStdOut output;
			using (output = new CaptureStdOut())
			{
				await Program.Main(new string[] { "--file", "OneElementWithSizes.html" });
			}

			// CI debugging statements
			if (!string.IsNullOrEmpty(output.StdErr))
			{
				Console.WriteLine("StdErr:");
				Console.WriteLine(output.StdErr);
			}
			Console.WriteLine("StdOut:");
			Console.WriteLine(output.StdOut);


			Assert.AreEqual("", output.StdErr);
			string expected = @"0,0,800,316.5
8,8,784,300.5
8,8,400.29688,300.5
0,0,0,0
".Replace("\r", "");
			Assert.AreEqual(expected, output.StdOut);
		}
	}

}

class CaptureStdOut : IDisposable
{
	private readonly TextWriter originalStdOut;
	private readonly TextWriter originalStdErr;
	private readonly TextWriter tmpStdOut;
	private readonly TextWriter tmpStdErr;

	public string? StdOut { get; private set; }
	public string? StdErr { get; private set; }

	public CaptureStdOut()
	{
		this.originalStdOut = Console.Out;
		this.tmpStdOut = new StringWriter();
		Console.SetOut(this.tmpStdOut);

		this.originalStdErr = Console.Error;
		this.tmpStdErr = new StringWriter();
		Console.SetError(tmpStdErr);
	}
	public void Dispose()
	{
		Console.SetOut(originalStdOut);
		this.StdOut = tmpStdOut.ToString();
		this.tmpStdOut.Dispose();

		Console.SetError(originalStdErr);
		this.StdErr = tmpStdErr.ToString();
		this.tmpStdErr.Dispose();
	}
}