using System.Collections.Concurrent;

namespace MicroserviceFramework.AspNetCore.Filters
{
	/// <summary>
	/// Filter 的顺序，越大则先运行
	/// </summary>
	public static class Conts
	{
		public const int UnitOfWork = 1000;
		public const int Audit = 2000;

		public static readonly ConcurrentDictionary<string, object> MethodDict;

		static Conts()
		{
			MethodDict = new ConcurrentDictionary<string, object>();
			MethodDict.TryAdd("POST", null);
			MethodDict.TryAdd("DELETE", null);
			MethodDict.TryAdd("PATCH", null);
			MethodDict.TryAdd("PUT", null);
		}
	}
}