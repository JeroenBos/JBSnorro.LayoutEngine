using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace SeleniumLayoutEngine.Tests
{
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
			string expected = @"0,0,800,600
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
			string expected = @"0,0,800,316.5
8,8,784,300.5
8,8,400.29688,300.5
0,0,0,0
".Replace("\r", "");
			string stdOut = SkipCIConnectionFailedLines(output.StdOut!);
			Assert.AreEqual(expected, stdOut);
		}
	}
	static class Extensions
	{
		/// <summary> Returns the index of the first element matching the specified predicate. Returns -1 if no elements match it. </summary>
		/// <typeparam name="T"> The type of the elements. </typeparam>
		/// <param name="sequence"> The elements to check for a match. </param>
		/// <param name="predicate"> The function determining whether an element matches. </param>
		public static int IndexOf<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
		{
			return sequence.IndexOf((element, i) => predicate(element));
		}
		/// <summary> Returns the index of the first element matching the specified predicate. Returns -1 if no elements match it. </summary>
		/// <typeparam name="T"> The type of the elements. </typeparam>
		/// <param name="sequence"> The elements to check for a match. </param>
		/// <param name="predicate"> The function determining whether an element matches. </param>
		public static int IndexOf<T>(this IEnumerable<T> sequence, Func<T, int, bool> predicate)
		{
			if (sequence == null) throw new ArgumentNullException();
			if (predicate == null) throw new ArgumentNullException();

			int i = 0;
			foreach (T element in sequence)
			{
				if (predicate(element, i))
					return i;
				i++;
			}

			return -1;
		}
	}
}
