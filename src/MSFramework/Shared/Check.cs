using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroserviceFramework.Shared
{
	/// <summary>
	/// 参数合法性检查类
	/// </summary>
	public static class Check
	{
		public static T NotNull<T>(T value, string parameterName)
		{
#pragma warning disable IDE0041 // Use 'is null' check
			if (ReferenceEquals(value, null))
#pragma warning restore IDE0041 // Use 'is null' check
			{
				NotEmpty(parameterName, nameof(parameterName));

				throw new ArgumentNullException(parameterName);
			}

			return value;
		}

		public static IReadOnlyList<T> NotEmpty<T>(IReadOnlyList<T> value,
			string parameterName)
		{
			NotNull(value, parameterName);

			if (value.Count == 0)
			{
				NotEmpty(parameterName, nameof(parameterName));

				throw new ArgumentException($"Collection is empty {parameterName}");
			}

			return value;
		}

		public static string NotEmpty(string value, string parameterName)
		{
			Exception e = null;
			if (value is null)
			{
				e = new ArgumentNullException(parameterName);
			}
			else if (value.Trim().Length == 0)
			{
				e = new ArgumentException($"Argument is empty {parameterName}");
			}

			if (e != null)
			{
				NotEmpty(parameterName, nameof(parameterName));

				throw e;
			}

			return value;
		}

		public static string NullButNotEmpty(string value, string parameterName)
		{
			if (!(value is null)
			    && value.Length == 0)
			{
				NotEmpty(parameterName, nameof(parameterName));

				throw new ArgumentException($"Argument is empty {parameterName}");
			}

			return value;
		}

		public static IReadOnlyList<T> HasNoNulls<T>(IReadOnlyList<T> value,
			string parameterName)
			where T : class
		{
			NotNull(value, parameterName);

			if (value.Any(e => e == null))
			{
				NotEmpty(parameterName, nameof(parameterName));

				throw new ArgumentException(parameterName);
			}

			return value;
		}


		public static IReadOnlyList<string> HasNoEmptyElements(IReadOnlyList<string> value,
			string parameterName)
		{
			NotNull(value, parameterName);

			if (value.Any(string.IsNullOrWhiteSpace))
			{
				NotEmpty(parameterName, nameof(parameterName));

				throw new ArgumentException($"Has no empty elements in {parameterName}");
			}

			return value;
		}

		public static void DebugAssert(bool condition, string message)
		{
			if (!condition)
			{
				throw new Exception($"Check.DebugAssert failed: {message}");
			}
		}
	}
}