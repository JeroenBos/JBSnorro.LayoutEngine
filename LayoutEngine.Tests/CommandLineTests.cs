using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using JBSnorro;
using JBSnorro.Web;

public class CommandLineTests
{
	private static string SkipCIConnectionFailedLines(string output)
	{
		string[] lines = output.Split('\n');
		int firstNonErrorLineIndex = lines.IndexOf(line => !line.StartsWith("Connection refused [::ffff:127.0.0.1]:"));
		if (firstNonErrorLineIndex == -1)
			firstNonErrorLineIndex = 0;

		// skip version number line
		if (lines[firstNonErrorLineIndex].StartsWith("LayoutEngine version"))
			firstNonErrorLineIndex++;
		return string.Join('\n', lines.Skip(firstNonErrorLineIndex));
	}
	[Test]
	public async Task Open_Index()
	{
		CaptureStdOut output;
		using (output = new CaptureStdOut())
		{
			await Program.Main(new string[] { "--no-cache", "--dir", "." });
		}

		#region CI debugging statements
		if (!string.IsNullOrEmpty(output.StdErr))
		{
			Console.WriteLine("StdErr:");
			Console.WriteLine(output.StdErr);
		}
		//Console.WriteLine("StdOut:");
		//Console.WriteLine(output.StdOut);
		#endregion

		Assert.AreEqual("", output.StdErr);
		string expected = @"########## RECTANGLES INCOMING (V1) ##########
HTML,0,0,800,600
BODY,0,0,800,600
DIV,0,0,800,0
HEAD,0,0,0,0
LINK,0,0,0,0
META,0,0,0,0
STYLE,0,0,0,0
".Replace("\r", "");
		string stdOut = SkipCIConnectionFailedLines(output.StdOut!);
		if (expected != stdOut)
        {
			Console.WriteLine("output.stdOut");
			Console.WriteLine(stdOut);
			Console.WriteLine(stdOut.StartsWith("Connection refused [::ffff:127.0.0.1]:"));
		}
		Assert.AreEqual(expected, stdOut);
	}
	[Test]
	public async Task Open_One_Element_With_Sizes_Print_The_Size()
	{
		CaptureStdOut output;
		using (output = new CaptureStdOut())
		{
			await Program.Main(new string[] { "--no-cache", "--file", "OneElementWithSizes.html" });
		}

		#region CI debugging statements
		if (!string.IsNullOrEmpty(output.StdErr))
		{
			Console.WriteLine("StdErr:");
			Console.WriteLine(output.StdErr);
		}
		//Console.WriteLine("StdOut:");
		//Console.WriteLine(output.StdOut);
		#endregion


		Assert.AreEqual("", output.StdErr);
		string expected = @"########## RECTANGLES INCOMING (V1) ##########
HTML,0,0,800,316.5
BODY,8,8,784,300.5
DIV,8,8,400.29688,300.5
HEAD,0,0,0,0
".Replace("\r", "");
		string stdOut = SkipCIConnectionFailedLines(output.StdOut!);
		if (expected != stdOut)
		{
			Console.WriteLine("output.stdOut");
			Console.WriteLine(stdOut);
			Console.WriteLine(stdOut.StartsWith("Connection refused [::ffff:127.0.0.1]:"));
		}
		Assert.AreEqual(expected, stdOut);
	}
}
