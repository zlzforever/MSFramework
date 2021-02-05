namespace MicroserviceFramework.Utilities
{
	public static class StringUtilities
	{
		public static unsafe string ToCamelCase(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return value;
			}

			fixed (char* chr = value)
			{
				var valueChar = *chr;
				*chr = char.ToLowerInvariant(valueChar);
			}

			return value;
		}
	}
}