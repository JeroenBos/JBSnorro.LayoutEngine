using System;
using System.Collections.Generic;
using System.IO;
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
		public static nint ComputeHash(this byte[]? data)
		{
			if (data == null)
			{
				return 0;
			}

			nint i = data.Length;
			nint hc = i + 1;

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
		public static nint ComputeHash(this string data)
		{
			if (data == null)
			{
				return 0;
			}

			int i = data.Length;
			nint hc = i + 1;

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
		public static nint ComputeFileHash(this string path)
		{
			var bytes = File.ReadAllBytes(path);
			return bytes.ComputeHash();
		}
		/// <summary>
		/// Hashes the contents of the specified file.
		/// </summary>
		public static async Task<nint> ComputeFileHashCodeAsync(this string path)
		{
			var bytes = await File.ReadAllBytesAsync(path);
			return bytes.ComputeHash();
		}
		/// <summary>
		/// Sums all specified numbers, without overflow exceptions.
		/// </summary>
		public static nint Sum(this IEnumerable<nint> summands)
		{
			nint result = 0;
			foreach (var summand in summands)
				result += summand;
			return result;
		}
	}
}
