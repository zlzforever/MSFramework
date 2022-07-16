using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MicroserviceFramework.Runtime
{
	public static class StringExtensions
	{
		public static string ToHex(this IEnumerable<byte> bytes)
		{
			var builder = new StringBuilder();
			foreach (var b in bytes)
			{
				builder.Append($"{b:x2}");
			}

			return builder.ToString();
		}

		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}

		public static bool IsNullOrWhiteSpace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		public static string ToSnakeCase(this string s) => ToSeparatedCase(s, '_');

		public static unsafe string ToCamelCase(this string value)
		{
			if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
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

		private enum SeparatedCaseState
		{
			Start,
			Lower,
			Upper,
			NewWord,
		}

		private static string ToSeparatedCase(string s, char separator)
		{
			if (string.IsNullOrEmpty(s))
				return s;
			var stringBuilder = new StringBuilder();
			var separatedCaseState = SeparatedCaseState.Start;
			for (var index = 0; index < s.Length; ++index)
			{
				if (s[index] == ' ')
				{
					if (separatedCaseState != SeparatedCaseState.Start)
						separatedCaseState = SeparatedCaseState.NewWord;
				}
				else if (char.IsUpper(s[index]))
				{
					switch (separatedCaseState)
					{
						case SeparatedCaseState.Lower:
						case SeparatedCaseState.NewWord:
							stringBuilder.Append(separator);
							break;
						case SeparatedCaseState.Upper:
							var flag = index + 1 < s.Length;
							if (index > 0 & flag)
							{
								var c = s[index + 1];
								if (!char.IsUpper(c) && c != separator)
								{
									stringBuilder.Append(separator);
								}
							}

							break;
					}

					var lower = char.ToLower(s[index], CultureInfo.InvariantCulture);
					stringBuilder.Append(lower);
					separatedCaseState = SeparatedCaseState.Upper;
				}
				else if (s[index] == separator)
				{
					stringBuilder.Append(separator);
					separatedCaseState = SeparatedCaseState.Start;
				}
				else
				{
					if (separatedCaseState == SeparatedCaseState.NewWord)
						stringBuilder.Append(separator);
					stringBuilder.Append(s[index]);
					separatedCaseState = SeparatedCaseState.Lower;
				}
			}

			return stringBuilder.ToString();
		}
	}
}