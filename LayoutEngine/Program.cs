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
using System.Diagnostics;

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
				new Option<bool>(
                    alias: "--version",
                    description: "If specified, the version of this tool and the browser driver are printed. All other arguments are ignored then.",
                    getDefaultValue: () => false
                ).With(arity: Maybe.Some(ArgumentArity.ZeroOrOne)),
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
                ).With(arity: Maybe.Some(ArgumentArity.ZeroOrOne)),
                new Option<string?>(
                    alias: "--cache-path",
                    description: "The path to a file with cached results. ",
                    getDefaultValue: () => ".layoutenginecache/"
                ),
                new Option<bool>(
                    alias: "--headful",
                    description: "If true, the browser will pop up. In principle this doesn't matter, but in practice many things like getBoundingClientRect depend on headless or not. This is more of a workaround than a feature. ",
                    getDefaultValue: () => false
                ).With(arity: Maybe.Some(ArgumentArity.ZeroOrOne)),
                new Option<int>(
                    alias: "--zoom",
                    description: "The zoom shouldn't affect the getBoundingClientRect of course, but unfortunately it does. Here you can specify which you want. More of a workaroudn than a feature. ",
                    getDefaultValue: () => 100
                ).With(arity: Maybe.Some(ArgumentArity.ZeroOrOne)),
            };

            if (args.Contains("--version"))
            {
                return PrintVersion();
            }
            // The error "An error occurred trying to start process 'dotnet-suggest' with working directory"
            // only occurs when running from Program.cs, not when running as test.
            // Try installing dotnet-suggest (globally)
            return new RootCommand("Copies all files matching patterns on modification/creation from source to dest")
            {
                Handler = CommandHandler.Create<string?, string?, bool, string, bool, int, CancellationToken>(main),
                Name = "layoutmeasurer",
            }.With(arguments).InvokeAsync(args);



            /// <param name="cancellationToken"> Canceled on e.g. process exit or Ctrl+C events. </param>
            async Task main(string? dir, string? file, bool noCache, string cachePath, bool headful, int zoom, CancellationToken cancellationToken)
            {
                await EnsureDriverExtracted();
                Console.Out.WriteLine($"LayoutEngine version {Assembly.GetExecutingAssembly().GetName().Version!.ToString(3)}");

                // for these weird lines, see https://github.com/dotnet/command-line-api/issues/1360#issuecomment-886983870
                // I think by virtue of not being able to specify the empty string as argument on the command line, this works.
                if (dir == "") dir = null;
                if (file == "") file = null;
                bool headless = !headful;

                cancellationToken.ThrowIfCancellationRequested();

                if (dir == null && file == null)
                {
                    throw new ArgumentException("Either --dir or --file must be specified");
                }
                if (zoom < 25 || zoom > 500)
                {
                    throw new ArgumentException("--zoom must be in [25, 500].");
                }
                if (file != null)
                    file = Path.GetFullPath(file);

                var cache = noCache ? null : new Cache(cachePath, headless: !headful, zoom);
                var (rectangles, hash) = cache == null ? (null, null) : await cache.TryGetValue(file, dir);
                if (rectangles == null)
                {
                    using var driver = dir != null ? LayoutEngine.OpenDir(dir, headful, zoom) : LayoutEngine.OpenPage(file!, headful, zoom);
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
            async Task<int> PrintVersion()
            {
                Console.Out.WriteLine($"LayoutEngine version {Assembly.GetExecutingAssembly().GetName().Version!.ToString(3)}");

                // installer doesn't support --version unfortunately // await new ProcessStartInfo(GetInstallerPath(), "--version").WaitForExitAndReadOutputAsync();

                // driver
                string driverPath = await EnsureDriverExtracted();
                var (exitCode, stdOut, stdErr) = await new ProcessStartInfo(driverPath, "--version").WaitForExitAndReadOutputAsync();
                if (exitCode == 0)
                {
                    Console.Out.WriteLine(stdOut);
                }
                else
                {
                    Console.Error.WriteLine(stdErr);
                    return exitCode;
                }

                // publication artifact
                Console.Out.WriteLine("Publication artifact version:");
                (exitCode, stdOut, stdErr)  = await new ProcessStartInfo(GetPublicationArtifactPath(), "--version").WaitForExitAndReadOutputAsync();
                if (exitCode == 0)
                {
                    Console.Out.WriteLine(stdOut);
                }
                else
                {
                    Console.Error.WriteLine(stdErr);
                    return exitCode;
                }
                return 0;
            }
        }
        public static Task<string> EnsureDriverExtracted(string dir = "./")
        {
            return EnsureDriverExtracted(dir: dir, extension: OperatingSystem.IsWindows() ? ".exe" : "");
        }
        internal static async Task<string> EnsureDriverExtracted(string extension, string dir = "./")
        {
            var driverPath = GetChromedriverPath(extension, dir);

            if (!File.Exists(driverPath))
            {
                File.WriteAllBytes(driverPath, Resources.chromedriver);
            }

            if (!OperatingSystem.IsWindows())
            {
                string bash = $"chmod +xwr '{driverPath}'";
                var output = await ProcessExtensions.WaitForExitAndReadOutputAsync("bash", "-c", '"' + bash + '"');

                if (output.ExitCode != 0)
                {
                    Console.WriteLine($"Error ({output.ExitCode}) in settings executable bit on chromedriver");
                    Console.WriteLine(output.ErrorOutput);
                }
            }
            return driverPath;
        }
        internal static string GetChromedriverPath()
        {
            string extension = OperatingSystem.IsWindows() ? ".exe" : "";
            return GetChromedriverPath(extension);
        }
        internal static string GetChromedriverPath(string extension, string dir = "./")
        {
            string filename = "chromedriver" + extension;
            string path = Path.GetFullPath(Path.Combine(dir, filename));
            return path;
        }
        internal static string GetInstallerPath()
        {
            string dir = "./installers/";
            string filename = OperatingSystem.IsWindows() ? "ChromeStandaloneSetup64.exe" : "google-chrome-stable_current_amd64.deb";
            string path = Path.Combine(dir, filename);
            return path;
        }
        internal static string GetPublicationArtifactPath()
        {
            string path = "./publish/LayoutEngine" + (OperatingSystem.IsWindows() ? ".exe" : "");
            return path;
        }
    }
}
