using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JBSnorro;

namespace JBSnorro.Web
{
	class Cache
	{
		private CacheFile? cache;
		private string? cachePath;

		private static readonly nint layoutEngineVersionHash = Assembly.GetExecutingAssembly().GetName().Version?.GetHashCode() ?? (nint)0;
		/// <summary>
		/// Gets the rectangles if if exists in the cache.
		/// </summary>
		public async Task<(IEnumerable<RectangleF>? Rectangles, string Hash)> TryGetValue(string? file, string? dir, string cacheFilePath)
		{
			if (file is null == dir is null)
				throw new ArgumentException("Either file or dir must be provided");

			var hashTask = ComputeHash(file, dir);

			if (this.cache == null)
			{
				this.cachePath = cacheFilePath;
				this.cache = await CacheFile.ReadFrom(cacheFilePath);
			}

			var hash = (await hashTask).ToString();

			if (this.cache.entries.TryGetValue(hash, out var entry))
			{
				return (entry.Rectangles, hash);
			}
			return (null, hash);
		}
		public async Task AppendAndFlush(string? file, string? dir, string hash, IEnumerable<RectangleF> rectangles, string cacheFilePath)
		{
			if (file is null == dir is null)
				throw new ArgumentException("Either file or dir must be provided");
			if (this.cache == null)
			{
				this.cachePath = cacheFilePath;
				this.cache = await CacheFile.ReadFrom(cacheFilePath);
			}
			else if (this.cachePath != cacheFilePath)
				throw new ArgumentException("this.cachePath != cacheFilePath");

			this.cache.entries[hash] = new CacheFile.CacheEntry() { Hash = hash, Rectangles = rectangles };
			await this.cache.WriteTo(cacheFilePath);
		}

		public static async Task<nint> ComputeHash(string? file, string? dir)
		{
			if (dir != null)
				dir = Path.GetFullPath(dir);
			var allRelevantFilenames = file != null ? new string[] { file } : dir!.GetAllFilenamesRecursively();
			var subfilenames = file != null ? new string[] { "" } : allRelevantFilenames.Select(path => Path.GetRelativePath(dir!, path));

			var hashCodeTasks = allRelevantFilenames.Zip(subfilenames, (fullpath, subpath) => Task.Run(() => fullpath.ComputeFileHash() + subpath.ComputeHash()));
			var hashCodes = await Task.WhenAll(hashCodeTasks);

			var sum = hashCodes.Sum();
			var versionHash = layoutEngineVersionHash;
			return (nint)sum + versionHash;
		}
	}

	class CacheFile
	{
		internal const string newEntryPrefix = "#";
		internal readonly Dictionary<string, CacheEntry> entries;
		public static async Task<CacheFile> ReadFrom(string path)
		{
			var lines = File.Exists(path) ? (await File.ReadAllLinesAsync(path)).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray() : Array.Empty<string>();
			return new CacheFile(CacheEntry.Parse(lines).Select(entry => KeyValuePair.Create(entry.Hash, entry)));
		}

		public CacheFile(IEnumerable<KeyValuePair<string, CacheEntry>> entries)
		{
			this.entries = new Dictionary<string, CacheEntry>(entries);
		}

		public Task WriteTo(string path)
		{
			// using var file = File.OpenWrite(path);

			var lines = entries.SelectMany(entry => entry.Value.Lines);
			return File.WriteAllLinesAsync(path, lines);
		}

		public record CacheEntry
		{
			public string Hash { get; internal init; } = default!;
			public string[] Output { get; internal init; } = default!;

			internal static IEnumerable<CacheEntry> Parse(string[] lines)
			{
				for (int index = 0; index < lines.Length; index++)
				{
					string line = lines[index];
					if (line.StartsWith(newEntryPrefix))
					{
						int endIndex = index + 1 + lines.Skip(index + 1).IndexOf(l => l.StartsWith(newEntryPrefix));
						if (endIndex == index) // i.e. IndexOf returned -1
							endIndex = lines.Length;
						yield return new CacheEntry
						{
							Hash = line[newEntryPrefix.Length..],
							Output = lines[(index + 1)..endIndex]
						};
					}
				}
			}

			public IEnumerable<RectangleF> Rectangles
			{
				get
				{
					return Output.Select(line =>
					{
						var values = line.Split(",").Select(float.Parse).ToArray();
						if (values.Length != 4)
							throw new Exception("Expected 4 values for rectangle");
						return new RectangleF(values[0], values[1], values[2], values[3]);
					});
				}
				init
				{
					if (this.Output != null)
						throw new ArgumentException("Rectangles can only be set when Output isn't");

					this.Output = value.Select(BoundingRectMeasurerExtensions.Format).ToArray();
				}
			}
			public IEnumerable<string> Lines
			{
				get
				{
					yield return newEntryPrefix + Hash;
					foreach (var outputLine in Output)
						yield return outputLine;
				}
			}
		}
	}
}
