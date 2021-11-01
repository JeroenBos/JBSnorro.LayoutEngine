using System.IO;
using System.Runtime.CompilerServices;

public static class Properties
{
	public static string GetCurrentFileName([CallerFilePath] string? fileName = null)
	{
		return fileName!;
	}
	public static string RepoRoot
	{
		get
		{
			string currentFilePath = GetCurrentFileName();
			string repoRootRelative = Path.Combine(currentFilePath, "../../..");
			string repoRootAbsolute = Path.GetFullPath(repoRootRelative);
			return repoRootAbsolute;
		}
	}
}
