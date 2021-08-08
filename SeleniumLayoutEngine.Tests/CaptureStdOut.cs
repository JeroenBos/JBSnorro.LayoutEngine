using System.IO;
using System;

class CaptureStdOut : IDisposable
{
	private readonly TextWriter originalStdOut;
	private readonly TextWriter originalStdErr;
	private readonly TextWriter tmpStdOut;
	private readonly TextWriter tmpStdErr;

	public string? StdOut { get; private set; }
	public string? StdErr { get; private set; }

	public CaptureStdOut()
	{
		this.originalStdOut = Console.Out;
		this.tmpStdOut = new StringWriter();
		Console.SetOut(this.tmpStdOut);

		this.originalStdErr = Console.Error;
		this.tmpStdErr = new StringWriter();
		Console.SetError(tmpStdErr);
	}
	public void Dispose()
	{
		Console.SetOut(originalStdOut);
		this.StdOut = tmpStdOut.ToString();
		this.tmpStdOut.Dispose();

		Console.SetError(originalStdErr);
		this.StdErr = tmpStdErr.ToString();
		this.tmpStdErr.Dispose();
	}
}