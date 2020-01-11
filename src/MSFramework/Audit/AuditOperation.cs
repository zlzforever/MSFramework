using System;
using System.Collections.Generic;

namespace MSFramework.Audit
{
	public class AuditOperation : AuditBase
	{
		/// <summary>
		/// 初始化一个<see cref="AuditOperation"/>类型的新实例
		/// </summary>
		public AuditOperation()
		{
			EntityEntries = new List<AuditEntity>();
		}

		/// <summary>
		/// 获取或设置 执行的功能名
		/// </summary>
		public string FunctionName { get; set; }

		/// <summary>
		/// 获取或设置 当前访问IP
		/// </summary>
		public string Ip { get; set; }

		/// <summary>
		/// 获取或设置 当前访问UserAgent
		/// </summary>
		public string UserAgent { get; set; }

		/// <summary>
		/// 获取或设置 消息
		/// </summary>
		public string Message { get; set; }
		
		/// <summary>
		/// 获取或设置 结束时间
		/// </summary>
		public DateTime EndedTime { get; set; }

		/// <summary>
		/// 获取或设置 审计数据信息集合
		/// </summary>
		public ICollection<AuditEntity> EntityEntries { get; set; }
	}
}