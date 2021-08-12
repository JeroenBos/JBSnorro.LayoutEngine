using JBSnorro;

namespace System.CommandLine
{
	public static class CommandLineExtensions
	{
		public static RootCommand With(this RootCommand rootCommand, params Symbol[] symbols)
		{
			foreach (Symbol symbol in symbols)
				rootCommand.Add(symbol);
			return rootCommand;
		}
		public static Argument<T> With<T>(this Argument<T> argument,
										  Maybe<string> description = default,
										  Maybe<T> defaultValue = default,
										  Maybe<bool> required = default)
		{
			if (description.HasValue)
				argument.Description = description.Value;
			if (defaultValue.HasValue)
				argument.SetDefaultValue(defaultValue.Value);

			return argument;
		}

		public static Option<T> With<T>(this Option<T> option,
										Maybe<string> description = default,
										Maybe<T> defaultValue = default,
										Maybe<bool> required = default)
		{
			if (description.HasValue)
				option.Description = description.Value;
			if (defaultValue.HasValue)
				option.SetDefaultValue(defaultValue.Value);
			if (required.HasValue)
				option.IsRequired = required.Value;

			return option;
		}
	}
}
