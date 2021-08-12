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
		return string.Join('\n', lines.Skip(firstNonErrorLineIndex));
	}
	[Test]
	public async Task Open_Index()
	{
		CaptureStdOut output;
		using (output = new CaptureStdOut())
		{
			await Program.Main(new string[] { "--dir", "." });
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
		string expected = @"########## RECTANGLES INCOMING ##########
0,0,800,600
0,0,800,600
0,0,800,0
0,0,0,0
0,0,0,0
0,0,0,0
0,0,0,0
".Replace("\r", "");
		string stdOut = SkipCIConnectionFailedLines(output.StdOut!);
		Assert.AreEqual(expected, stdOut);
	}
	[Test]
	public async Task Open_One_Element_With_Sizes_Print_The_Size()
	{
		CaptureStdOut output;
		using (output = new CaptureStdOut())
		{
			await Program.Main(new string[] { "--file", "OneElementWithSizes.html" });
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
		string expected = @"########## RECTANGLES INCOMING ##########
0,0,800,316.5
8,8,784,300.5
8,8,400.29688,300.5
0,0,0,0
".Replace("\r", "");
		string stdOut = SkipCIConnectionFailedLines(output.StdOut!);
		Assert.AreEqual(expected, stdOut);
	}
}
