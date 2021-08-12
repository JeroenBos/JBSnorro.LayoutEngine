using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine.Builder;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using System.Text;
using System.Globalization;
using System.Drawing;
using Newtonsoft.Json;

namespace JBSnorro.Web
{
	internal class Program
	{
		public static Task<int> Main(string[] args)
		{
			Console.Out.NewLine = "\n";
			Console.Error.NewLine = "\n";
			EnsureDriverExtracted();

			// the CLI arguments and options
			var arguments = new Symbol[]
			{
			// I tried using FileInfo and DirectoryInfo, but then if the argument is not provided, System.CommandLine throws a "path is empty" exception
			new Option<string?>(
				alias: "--dir",
				description: "The absolute path to a directory of the html file to process. Must contain index.html.",
				getDefaultValue: () => null
			),
			new Option<string?>(
				alias: "--file",
				description: "The html file to process.",
				getDefaultValue: () => null
			),
			};

			return new RootCommand("Copies all files matching patterns on modification/creation from source to dest")
			{
				Handler = CommandHandler.Create<string?, string?, CancellationToken>(main),
				Name = "layoutmeasurer",
			}.With(arguments).InvokeAsync(args);



			/// <param name="cancellationToken"> Canceled on e.g. process exit or Ctrl+C events. </param>
			void main(string? dir, string? file, CancellationToken cancellationToken)
			{
				// for these weird lines, see https://github.com/dotnet/command-line-api/issues/1360#issuecomment-886983870
				// I think by virtue of not being able to specify the empty string as argument on the command line, this works.
				if (dir == "") dir = null;
				if (file == "") file = null;

				cancellationToken.ThrowIfCancellationRequested();

				if (dir == null && file == null)
				{
					throw new ArgumentException("Either --dir or --file must be specified");
				}
				if (file != null)
					file = Path.GetFullPath(file);

				using var driver = dir != null ? LayoutEngine.OpenDir(dir) : LayoutEngine.OpenPage(file!);
				cancellationToken.ThrowIfCancellationRequested();

				var rectangles = LayoutEngine.GetSortedMeasuredBoundingClientsRects(driver);
				cancellationToken.ThrowIfCancellationRequested();

				foreach (var rectangle in rectangles)
				{
					Console.WriteLine(rectangle.Format());
				}
			}
		}
		public static void EnsureDriverExtracted(string dir = "./")
		{
			string path = Path.GetFullPath(Path.Combine(dir, "chromedriver.exe"));
			if (!File.Exists(path))
			{
				File.WriteAllBytes(path, Resources.chromedriver);
			}
		}
	}
}