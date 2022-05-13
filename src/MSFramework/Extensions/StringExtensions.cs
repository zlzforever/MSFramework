using System.Globalization;
using System.Text;

namespace MicroserviceFramework.Extensions
{
	public static class StringExtensions
	{
		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}

		public static bool IsNullOrWhiteSpace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		public static string ToSnakeCase(this string s) => ToSeparatedCase(s, '_');

		// public static string ToCamelCase(this string s)
		// {
		// 	if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
		// 		return s;
		// 	var charArray = s.ToCharArray();
		// 	for (var index = 0; index < charArray.Length && (index != 1 || char.IsUpper(charArray[index])); ++index)
		// 	{
		// 		var flag = index + 1 < charArray.Length;
		// 		if (index > 0 & flag && !char.IsUpper(charArray[index + 1]))
		// 		{
		// 			if (char.IsSeparator(charArray[index + 1]))
		// 			{
		// 				charArray[index] = ToLower(charArray[index]);
		// 			}
		//
		// 			break;
		// 		}
		//
		// 		charArray[index] = ToLower(charArray[index]);
		// 	}
		//
		// 	return new string(charArray);
		// }

		private static char ToLower(char c)
		{
			c = char.ToLower(c, CultureInfo.InvariantCulture);
			return c;
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