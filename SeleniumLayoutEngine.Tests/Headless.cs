using OpenQA.Selenium.Chrome;

static class Extensions
{
	/// <summary>
	/// When running as CI, configures the options to run headlessly.
	/// </summary>
	public static void AsHeadlessInCI(this ChromeOptions options)
	{
#if CI
		options.AddArgument("--headless");
		options.AddArgument("--disable-gpu");
#endif
	}
}