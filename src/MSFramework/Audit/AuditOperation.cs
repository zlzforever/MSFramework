using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MSFramework.Common;
using MSFramework.Domain;
using MSFramework.EventBus;

namespace MSFramework.Audit
{
	public class AuditOperation : AggregateRootBase
	{
		/// <summary>
		/// 获取或设置 当前用户标识
		/// </summary>
		[StringLength(255)]
		public string UserId { get; set; }

		/// <summary>
		/// 获取或设置 当前用户名
		/// </summary>
		[StringLength(255)]
		public string UserName { get; set; }

		/// <summary>
		/// 获取或设置 当前用户昵称
		/// </summary>
		[StringLength(255)]
		public string NickName { get; set; }

		/// <summary>
		/// 获取或设置 信息添加时间
		/// </summary>
		public DateTimeOffset CreatedTime { get; set; }

		/// <summary>
		/// 获取或设置 执行的功能名
		/// </summary>
		[StringLength(255)]
		public string FunctionName { get; set; }

		/// <summary>
		/// 获取或设置 执行的功能名
		/// </summary>
		[StringLength(255)]
		public string FunctionPath { get; set; }
		
		/// <summary>
		/// 获取或设置 当前访问IP
		/// </summary>
		[StringLength(40)]
		public string Ip { get; set; }

		/// <summary>
		/// 获取或设置 当前访问UserAgent
		/// </summary>
		[StringLength(255)]
		public string UserAgent { get; set; }

		/// <summary>
		/// 获取或设置 消息
		/// </summary>
		[StringLength(500)]
		public string Message { get; set; }

		/// <summary>
		/// 获取或设置 结束时间
		/// </summary>
		public DateTimeOffset EndedTime { get; set; }

		/// <summary>
		/// 获取或设置 执行耗时，单位毫秒
		/// </summary>
		public int Elapsed { get; set; }

		/// <summary>
		/// 获取或设置 审计数据信息集合
		/// </summary>
		public virtual ICollection<AuditEntity> Entities { get; set; } = new List<AuditEntity>();
	}
}