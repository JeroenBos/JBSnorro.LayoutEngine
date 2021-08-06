using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using static System.Net.WebRequestMethods;

public interface IMeasurer<out T>
{
	/// <summary>
	/// Measures the sizes of the 
	/// </summary>
	/// <param name="path"></param>
	T? Measure(string path);
}
