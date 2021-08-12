using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JBSnorro.Web
{
	/// <summary>
	/// Redirects the stdout to stderr.
	/// </summary>
	class StdOutRedirecter : IDisposable
	{
		private readonly bool ownsSink;
		private readonly TextWriter originalStdOut;
		private readonly TextWriter sink;

		public static StdOutRedirecter RedirectToStdErr()
		{
			var redirecter = new StringWriterRedirecter(Console.Error);
			return new StdOutRedirecter(redirecter, ownsSink: true);
		}
		public static StdOutRedirecter RedirectTo(TextWriter output)
		{
			return new StdOutRedirecter(output, ownsSink: false);
		}
		public static StdOutRedirecter RedirectNowhere()
		{
			return new StdOutRedirecter(new StringWriter(), ownsSink: true);
		}
		private StdOutRedirecter(TextWriter sink, bool ownsSink)
		{
			this.ownsSink = ownsSink;
			this.sink = sink;
			this.originalStdOut = Console.Out;
			Console.SetOut(this.sink);
		}
		public void Dispose()
		{
			Console.SetOut(originalStdOut);
			if (ownsSink)
				this.sink.Dispose();
		}
	}
	class StdErrRedirecter : IDisposable
	{
		private readonly bool ownsSink;
		private readonly TextWriter originalStdOut;
		private readonly TextWriter sink;

		public static StdErrRedirecter RedirectNowhere()
		{
			return new StdErrRedirecter(new StringWriter(), ownsSink: true);
		}
		private StdErrRedirecter(TextWriter sink, bool ownsSink)
		{
			this.ownsSink = ownsSink;
			this.sink = sink;
			this.originalStdOut = Console.Out;
			Console.SetOut(this.sink);
		}
		public void Dispose()
		{
			Console.SetOut(originalStdOut);
			if (ownsSink)
				this.sink.Dispose();
		}
	}
	class StringWriterRedirecter : StringWriter
	{
		public TextWriter Output { get; }
		public StringWriterRedirecter(TextWriter output)
		{
			this.Output = output;
		}
		public override void Write(bool value) => Output.Write(value);
		public override void Write(char value) => Output.Write(value);
		public override void Write(char[] buffer, int index, int count) => Output.Write(buffer, index, count);
		public override void Write(char[]? buffer) => Output.Write(buffer);
		public override void Write(decimal value) => Output.Write(value);
		public override void Write(double value) => Output.Write(value);
		public override void Write(float value) => Output.Write(value);
		public override void Write(int value) => Output.Write(value);
		public override void Write(long value) => Output.Write(value);
		public override void Write(object? value) => Output.Write(value);
		public override void Write(ReadOnlySpan<char> buffer) => Output.Write(buffer);
		public override void Write(string format, object? arg0) => Output.Write(format, arg0);
		public override void Write(string format, object? arg0, object? arg1) => Output.Write(format, arg0, arg1);
		public override void Write(string format, object? arg0, object? arg1, object? arg2) => Output.Write(format, arg0, arg1, arg2);
		public override void Write(string format, params object?[] arg) => Output.Write(format, arg);
		public override void Write(string? value) => Output.Write(value);
		public override void Write(StringBuilder? value) => Output.Write(value);
		public override void Write(uint value) => Output.Write(value);
		public override void Write(ulong value) => Output.Write(value);
		public override Task WriteAsync(char value) => Output.WriteAsync(value);
		public override Task WriteAsync(char[] buffer, int index, int count) => Output.WriteAsync(buffer, index, count);
		public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default) => Output.WriteAsync(buffer, cancellationToken);
		public override Task WriteAsync(string? value) => Output.WriteAsync(value);
		public override Task WriteAsync(StringBuilder? value, CancellationToken cancellationToken = default) => Output.WriteAsync(value, cancellationToken);
		public override void WriteLine() => Output.WriteLine();
		public override void WriteLine(bool value) => Output.WriteLine(value);
		public override void WriteLine(char value) => Output.WriteLine(value);
		public override void WriteLine(char[] buffer, int index, int count) => Output.WriteLine(buffer, index, count);
		public override void WriteLine(char[]? buffer) => Output.WriteLine(buffer);
		public override void WriteLine(decimal value) => Output.WriteLine(value);
		public override void WriteLine(double value) => Output.WriteLine(value);
		public override void WriteLine(float value) => Output.WriteLine(value);
		public override void WriteLine(int value) => Output.WriteLine(value);
		public override void WriteLine(long value) => Output.WriteLine(value);
		public override void WriteLine(object? value) => Output.WriteLine(value);
		public override void WriteLine(ReadOnlySpan<char> buffer) => Output.WriteLine(buffer);
		public override void WriteLine(string format, object? arg0) => Output.WriteLine(format, arg0);
		public override void WriteLine(string format, object? arg0, object? arg1) => Output.WriteLine(format, arg0, arg1);
		public override void WriteLine(string format, object? arg0, object? arg1, object? arg2) => Output.WriteLine(format, arg0, arg1, arg2);
		public override void WriteLine(string format, params object?[] arg) => Output.WriteLine(format, arg);
		public override void WriteLine(string? value) => Output.WriteLine(value);
		public override void WriteLine(StringBuilder? value) => Output.WriteLine(value);
		public override void WriteLine(uint value) => Output.WriteLine(value);
		public override void WriteLine(ulong value) => Output.WriteLine(value);
		public override Task WriteLineAsync() => Output.WriteLineAsync();
		public override Task WriteLineAsync(char value) => Output.WriteLineAsync(value);
		public override Task WriteLineAsync(char[] buffer, int index, int count) => Output.WriteLineAsync(buffer, index, count);
		public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default) => Output.WriteLineAsync(buffer, cancellationToken);
		public override Task WriteLineAsync(string? value) => Output.WriteLineAsync(value);
		public override Task WriteLineAsync(StringBuilder? value, CancellationToken cancellationToken = default) => Output.WriteLineAsync(value, cancellationToken);
		public override void Flush() => Output.Flush();
		public override Task FlushAsync() => Output.FlushAsync();
	}
}
