using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;

namespace JBSnorro
{
	public static class Extensions
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

		public static string ToFileSystemPath(this string path)
		{
			// It seemed necessary first in Windows, although now it doesn't seem like it does. However, in CI it's still necessary
			if (!path.StartsWith("file:"))
				path = "file:///" + path;
			return path;
		}

		/// <summary> Gets whether the path is a full path in the current OS. </summary>
		/// <see href="https://stackoverflow.com/a/35046453/308451" />
		public static bool IsFullPath(string path)
		{
			if (OperatingSystem.IsWindows())
				return IsFullPathInWindows(path);
			else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
				return IsFullPathInUnix(path);

			throw new NotImplementedException("IsFullPath not implemented yet for current OS");
		}
		/// <summary> Gets whether the path is a full path. </summary>
		/// <see href="https://stackoverflow.com/a/2202096/308451"/>
		public static bool IsFullPathInUnix(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return false;

			// check if valid linux path:
			if (path.Contains((char)0))
				return false;

			// char 47 is '/', so we can skip checking it
			if (path.StartsWith("/"))
				return true;

			return false;
		}
		/// <summary> Gets whether the path is a full path. </summary>
		/// <see href="https://stackoverflow.com/a/35046453/308451" />
		public static bool IsFullPathInWindows(string path)
		{
			return !string.IsNullOrWhiteSpace(path)
				&& path.IndexOfAny(Path.GetInvalidPathChars()) == -1
				&& Path.IsPathRooted(path)
				&& !(Path.GetPathRoot(path)?.Equals("\\", StringComparison.Ordinal) ?? false);
		}
		/// <summary> Gets a new temporary directory. </summary>
		public static string CreateTemporaryDirectory()
		{
			string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempDirectory);
			return tempDirectory.Replace('\\', '/');
		}
		/// <summary>
		/// Quick and dirty get hash code implementation taking into account contents of the byte array.
		/// </summary>
		public static nuint ComputeHash(this byte[]? data)
		{
			if (data == null)
			{
				return 0;
			}

			int i = data.Length;
			nuint hc = (nuint)i + 1;

			while (--i >= 0)
			{
				hc *= 257;
				hc ^= data[i];
			}

			return hc;
		}
		/// <summary>
		/// Quick and dirty get hash code implementation taking into account contents of the string.
		/// </summary>
		public static nuint ComputeHash(this string data)
		{
			if (data == null)
			{
				return 0;
			}

			int i = data.Length;
			nuint hc = (nuint)i + 1;

			while (--i >= 0)
			{
				hc *= 257;
				hc ^= data[i];
			}

			return hc;
		}
		/// <summary>
		/// Gets all files in the directory, recursively.
		/// </summary>
		public static IEnumerable<string> GetAllFilenamesRecursively(this string dir, string pattern = "*")
		{
			return Directory.EnumerateFiles(dir, pattern, SearchOption.AllDirectories);
		}
		/// <summary>
		/// Hashes the contents of the specified file.
		/// </summary>
		public static nuint ComputeFileHash(this string path)
		{
			var bytes = File.ReadAllBytes(path);
			return bytes.ComputeHash();
		}
		/// <summary>
		/// Hashes the contents of the specified file.
		/// </summary>
		public static async Task<nuint> ComputeFileHashAsync(this string path)
		{
			var bytes = await File.ReadAllBytesAsync(path);
			return bytes.ComputeHash();
		}
		/// <summary>
		/// Sums all specified numbers, without overflow exceptions.
		/// </summary>
		public static nuint Sum(this IEnumerable<nuint> summands)
		{
			nuint result = 0;
			foreach (var summand in summands)
				result += summand;
			return result;
		}
		/// <summary>
		/// Returns the string enclosed by double quotes on each side.
		/// </summary>
		public static string WrapInDoubleQuotes(this string s)
		{
			return s.WrapIn("\"");
		}
		/// <summary>
		/// Returns the string enclosed by <paramref name="enclosing"/> on each side.
		/// </summary>
		public static string WrapIn(this string s, string enclosing)
		{
			return enclosing + s + enclosing;
		}
	}

	public static class ProcessExtensions
	{
		public static Task<ProcessOutput> WaitForExitAndReadOutputAsync(string executable, params string[] arguments)
		{
			return WaitForExitAndReadOutputAsync(executable, cancellationToken: default, arguments: arguments);
		}
		public static Task<ProcessOutput> WaitForExitAndReadOutputAsync(string executable, CancellationToken cancellationToken, params string[] arguments)
		{
			var process = new ProcessStartInfo(executable, string.Join(" ", arguments))
			{
				WorkingDirectory = Path.GetDirectoryName(executable),
			};
			return process.WaitForExitAndReadOutputAsync(cancellationToken);
		}
		public static async Task<ProcessOutput> WaitForExitAndReadOutputAsync(this ProcessStartInfo startInfo, int timeout)
		{
			using CancellationTokenSource cts = new();
			cts.CancelAfter(timeout);
			return await WaitForExitAndReadOutputAsync(startInfo, cts.Token);
		}

		public static async Task<ProcessOutput> WaitForExitAndReadOutputAsync(this ProcessStartInfo startInfo, CancellationToken cancellationToken = default)
		{
			using CancellationTokenSource cts = new();
			cancellationToken.Register(cts.Cancel);
			var task = impl(startInfo, cts.Token);
			return await task.WaitAsync(cts.Token);


			async Task<ProcessOutput> impl(ProcessStartInfo startInfo, CancellationToken cancellationToken)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var process = Process.Start(startInfo.WithOutput())!;

				cancellationToken.Register(process.Kill);
				cancellationToken.ThrowIfCancellationRequested();

				await process.WaitForExitAsync(cancellationToken);

				cancellationToken.ThrowIfCancellationRequested();

				string errorOutput = process.StandardError.ReadToEnd();
				string output = process.StandardOutput.ReadToEnd();
				return new ProcessOutput { ExitCode = process.ExitCode, StandardOutput = output, ErrorOutput = errorOutput };
			}
		}
		public static ProcessStartInfo WithHidden(this ProcessStartInfo startInfo)
		{
			startInfo.CreateNoWindow = true;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			return startInfo;
		}

		public static ProcessStartInfo WithOutput(this ProcessStartInfo startInfo)
		{
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			return startInfo;
		}
	}

	public record ProcessOutput
	{
		public int ExitCode { get; init; }
		public string StandardOutput { get; init; } = default!;
		public string ErrorOutput { get; init; } = default!;

		public void Deconstruct(out int exitCode, out string standardOutput, out string errorOutput)
		{
			exitCode = ExitCode;
			standardOutput = StandardOutput;
			errorOutput = ErrorOutput;
		}
		public static implicit operator ProcessOutput((int ExitCode, string StandardOutput, string StandardError) tuple)
		{
			return new ProcessOutput()
			{
				ExitCode = tuple.ExitCode,
				StandardOutput = tuple.StandardOutput,
				ErrorOutput = tuple.StandardError,
			};
		}
	}

}
