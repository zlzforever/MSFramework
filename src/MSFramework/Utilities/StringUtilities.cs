using System.Collections.Generic;
using System.Linq;

namespace MSFramework.Utilities
{
	public class StringUtilities
	{
		public static string ToUnixLike(string str)
		{
			var array = str.ToList();
			if (char.IsUpper(array[0]))
			{
				array[0] = char.ToLower(array[0]);
			}

			while (true)
			{
				var index = GetIndexOfFirstUpper(array);
				if (index <= 0)
				{
					break;
				}

				var c = array[index];
				array[index] = char.ToLower(c);
				if (array.ElementAtOrDefault(index - 1) != '_')
				{
					array.Insert(index, '_');
				}
			}

			return new string(array.ToArray());
		}

		private static int GetIndexOfFirstUpper(List<char> array)
		{
			var index = -1;
			for (var i = 0; i < array.Count; ++i)
			{
				if (!char.IsUpper(array[i]))
				{
					continue;
				}

				index = i;
				break;
			}

			return index;
		}
	}
}