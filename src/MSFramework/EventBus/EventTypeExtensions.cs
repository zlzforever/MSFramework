using System;
using System.Linq;

namespace MSFramework.EventBus
{
	public static class EventTypeExtensions
	{
		public static string GetEventName(this Type type)
		{
			string typeName;

			if (type.IsGenericType)
			{
				var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
				typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
			}
			else
			{
				typeName = type.Name;
			}

			return typeName;
		}
	}
}