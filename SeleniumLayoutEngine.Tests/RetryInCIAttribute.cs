using System;
using NUnit.Framework;


namespace SeleniumLayoutEngine.Tests
{
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
#if CI
	class RetryInCIAttribute : RetryAttribute
	{
		public RetryInCIAttribute(int retryCount = 3) : base(retryCount)
		{
		}
	}
#else
	class RetryInCIAttribute : Attribute
	{
	}
#endif
}
