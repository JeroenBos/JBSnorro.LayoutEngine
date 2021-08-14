using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBSnorro.Web
{
	public struct TaggedRectangle
	{
		public float X { get; init; }
		public float Y { get; init; }
		public float Width { get; init; }
		public float Height { get; init; }
		public string Tagname { get; init; }

		public TaggedRectangle(string tagname, float x, float Y, float width, float height)
		{
			this.Tagname = tagname;
			this.X = x;
			this.Y = Y;
			this.Width = width;
			this.Height = height;
		}
		/// <summary>
		/// Formats this rectangle, starting with the tag comma-separated with 4 comma-separated numbers.
		/// The decimal point is the period.
		/// </summary>
		public string Format()
		{
			return string.Join(',',
				this.Tagname,
				this.X.ToString(CultureInfo.InvariantCulture),
				this.Y.ToString(CultureInfo.InvariantCulture),
				this.Width.ToString(CultureInfo.InvariantCulture),
				this.Height.ToString(CultureInfo.InvariantCulture)
			);
		}
		public override bool Equals([NotNullWhen(true)] object? obj)
		{
			if (obj is TaggedRectangle other)
			{
				if (other.X != this.X)
					return false;
				if (other.Y != this.Y)
					return false;
				if (other.Width != this.Width)
					return false;
				if (other.Height != this.Height)
					return false;
				return string.Equals(other.Tagname, this.Tagname, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}
		public override int GetHashCode() => throw new NotImplementedException();
		public override string ToString() => Format();
	}
}
