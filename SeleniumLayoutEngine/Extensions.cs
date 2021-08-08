using System;
using System.Collections.Generic;
using System.IO;

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
}
