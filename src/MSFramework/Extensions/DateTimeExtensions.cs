using System;
using MSFramework.Common;

namespace MSFramework.Extensions
{
	public static class DateTimeExtensions
	{
		public static DateTime ToDateTime(long unixTime)
		{
			return DateTime2.Epoch.AddMilliseconds(unixTime);
		}

		public static DateTimeOffset ToDateTimeOffset(long unixTime)
		{
			return DateTime2.Epoch.AddMilliseconds(unixTime);
		}
	}
}