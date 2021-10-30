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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

namespace JBSnorro.Web
{
	internal class Program
	{
		public static Task<int> Main(string[] args)
		{
			Console.Out.NewLine = "\n";
			Console.Error.NewLine = "\n";

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
				new Option<bool>(
					alias: "--no-cache",
					description: "If specified, the cache will not be read nor written to.",
					getDefaultValue: () => false
				).With(arity: Maybe<IArgumentArity>.Some(ArgumentArity.ZeroOrOne)),
				new Option<string?>(
					alias: "--cache-path",
					description: "The path to a file with cached results. ",
					getDefaultValue: () => ".layoutenginecache/"
				),
				new Option<bool>(
					alias: "--headful",
					description: "If true, the browser will pop up. In principle this doesn't matter, but in practice many things like getBoundingClientRect depend on headless or not. This is more of a workaround than a feature",
					getDefaultValue: () => false
				).With(arity: Maybe<IArgumentArity>.Some(ArgumentArity.ZeroOrOne)),
			};

			// The error "An error occurred trying to start process 'dotnet-suggest' with working directory"
			// only occurs when running from Program.cs, not when running as test.
			// Try installing dotnet-suggest (globally)
			return new RootCommand("Copies all files matching patterns on modification/creation from source to dest")
			{
				Handler = CommandHandler.Create<string?, string?, bool, string, bool, CancellationToken>(main),
				Name = "layoutmeasurer",
			}.With(arguments).InvokeAsync(args);



			/// <param name="cancellationToken"> Canceled on e.g. process exit or Ctrl+C events. </param>
			async Task main(string? dir, string? file, bool noCache, string cachePath, bool headful, CancellationToken cancellationToken)
			{
				await EnsureDriverExtracted();
				Console.Out.WriteLine($"LayoutEngine version {Assembly.GetExecutingAssembly().GetName().Version!.ToString(3)}");

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

				var cache = noCache ? null : new Cache(cachePath, headless: !headful);
				var (rectangles, hash) = cache == null ? (null, null) : await cache.TryGetValue(file, dir);
				if (rectangles == null)
				{
					using var driver = dir != null ? LayoutEngine.OpenDir(dir) : LayoutEngine.OpenPage(file!);
					cancellationToken.ThrowIfCancellationRequested();

					rectangles = LayoutEngine.GetSortedMeasuredBoundingClientsRects(driver);
					if (cache != null)
					{
						await cache.Write(file, dir, hash!, rectangles);
					}
				}
				cancellationToken.ThrowIfCancellationRequested();

				Console.WriteLine("########## RECTANGLES INCOMING (V1) ##########");
				foreach (var rectangle in rectangles)
				{
					Console.WriteLine(rectangle.Format());
				}
			}
		}
		public static Task EnsureDriverExtracted(string dir = "./")
		{
			return EnsureDriverExtracted(dir: dir, extension: OperatingSystem.IsWindows() ? ".exe" : "");
		}
		internal static async Task EnsureDriverExtracted(string extension, string dir = "./")
		{
			string filename = "chromedriver" + extension;
			string path = Path.GetFullPath(Path.Combine(dir, filename));
			if (!File.Exists(path))
			{
				File.WriteAllBytes(path, Resources.chromedriver);
			}

			if (!OperatingSystem.IsWindows())
			{
				string bash = $"chmod +xwr '{path}'";
				var output = await ProcessExtensions.WaitForExitAndReadOutputAsync("bash", "-c", '"' + bash + '"');

				if (output.ExitCode != 0)
				{
					Console.WriteLine($"Error ({output.ExitCode}) in settings executable bit on chromedriver");
					Console.WriteLine(output.ErrorOutput);
				}
			}
		}
	}
}
