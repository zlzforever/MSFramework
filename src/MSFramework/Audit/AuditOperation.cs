using System;
using System.Collections.Generic;
using MSFramework.Common;
using MSFramework.Domain.AggregateRoot;

namespace MSFramework.Audit
{
	public class AuditOperation : CreationAuditedAggregateRoot<Guid>
	{
		/// <summary>
		/// 应用名称
		/// </summary>
		public string ApplicationName { get; private set; }

		/// <summary>
		/// 获取或设置 执行的功能名
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// 获取或设置 当前访问IP
		/// </summary>
		public string Ip { get; private set; }

		/// <summary>
		/// 获取或设置 当前访问UserAgent
		/// </summary>
		public string UserAgent { get; private set; }

		/// <summary>
		/// 获取或设置 审计数据信息集合
		/// </summary>
		public virtual ICollection<AuditEntity> Entities { get; private set; }

		public DateTimeOffset EndTime { get; private set; }

		public int Elapsed { get; private set; }

		private AuditOperation() : base(CombGuid.NewGuid())
		{
			Entities = new List<AuditEntity>();
		}

		public AuditOperation(string applicationName, string path, string ip, string userAgent) : this()
		{
			ApplicationName = applicationName;
			Path = path;
			Ip = ip;
			UserAgent = userAgent;
		}

		public void AddEntities(IEnumerable<AuditEntity> entities)
		{
			foreach (var entity in entities)
			{
				entity.Operation = this;
				Entities.Add(entity);
			}
		}

		public void End()
		{
			EndTime = DateTimeOffset.Now;
			if (CreationTime == default)
			{
				Elapsed = 0;
			}
			else
			{
				Elapsed = (int) (EndTime - CreationTime).TotalMilliseconds;
			}
		}

		public override string ToString()
		{
			return
				$"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'ApplicationName': {ApplicationName}, 'Path': {Path}, 'Ip': {Ip}, 'UserAgent': {UserAgent}, 'EndedTime': {EndTime:yyyy-MM-dd HH:mm:ss}, 'Elapsed': {Elapsed} }}";
		}
	}
}