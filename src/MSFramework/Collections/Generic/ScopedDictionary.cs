using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using MSFramework.Audit;
using MSFramework.Function;

namespace MSFramework.Collections.Generic
{
	/// <summary>
	/// 基于Scoped生命周期的数据字典
	/// </summary>
	public class ScopedDictionary : ConcurrentDictionary<string, dynamic>, IDisposable
	{
		/// <summary>
		/// 获取或设置 当前执行的功能
		/// </summary>
		public Function.Function Function { get; set; }
		
		/// <summary>
		/// 获取或设置 当前操作审计
		/// </summary>
		public AuditOperation AuditOperation { get; set; }

		/// <summary>
		/// 获取或设置 当前用户
		/// </summary>
		public ClaimsIdentity Identity { get; set; }

		/// <summary>释放资源.</summary>
		public void Dispose()
		{
			Function = null;
			AuditOperation = null;
			Identity = null;
		}
	}
}