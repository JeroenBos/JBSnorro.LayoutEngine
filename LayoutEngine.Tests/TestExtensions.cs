using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class TestExtensions
{
	public static string SkipCIConnectionFailedLines(this string output)
	{
		// Skips lines starting with `Connection refused [::ffff:127.0.0.1]:` or `LayoutEngine version`
		return string.Join('\n', output.Split('\n').SkipWhile(s => !s.StartsWith("######")));
	}
}
