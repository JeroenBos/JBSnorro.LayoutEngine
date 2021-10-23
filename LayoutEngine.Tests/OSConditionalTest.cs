using System.IO;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using JBSnorro;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

class TestOnWindowsOnly
#if WINDOWS
	: TestAttribute
#else
	: Attribute
#endif
{

}
class TestOnLinuxOnly
#if LINUX
	: TestAttribute
#else
	: Attribute
#endif
{

}

class DefineConstantsTests
{
#if WINDOWS
	[Test]
	public void TestOSWindows()
	{
		Assert.IsTrue(OperatingSystem.IsWindows());
		Assert.IsFalse(OperatingSystem.IsLinux());
	}
#else
	[Test]
	public void TestOSWindows()
	{
		Assert.IsFalse(OperatingSystem.IsWindows());
		Assert.IsTrue(OperatingSystem.IsLinux());
	}
#endif

#if LINUX
	[Test]
	public void TestOSLinux()
	{
		Assert.IsFalse(OperatingSystem.IsWindows());
		Assert.IsTrue(OperatingSystem.IsLinux());
	}
#else
	[Test]
	public void TestOSLinux()
	{
		Assert.IsTrue(OperatingSystem.IsWindows());
		Assert.IsFalse(OperatingSystem.IsLinux());
	}
#endif
}
