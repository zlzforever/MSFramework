using System;
using System.Collections.Generic;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Audit
{
	public class AuditOperation : CreationAggregateRoot<ObjectId>
	{
		/// <summary>
		/// 应用名称
		/// </summary>
		public string ApplicationName { get; private set; }

		/// <summary>
		/// 获取或设置执行的功能名
		/// </summary>
		public string Feature { get; private set; }

		/// <summary>
		/// 请求
		/// </summary>
		public string Url { get; private set; }

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

		private AuditOperation() : base(ObjectId.NewId())
		{
			Entities = new List<AuditEntity>();
		}

		public AuditOperation(string applicationName, string feature, string url, string ip, string userAgent) : this()
		{
			ApplicationName = applicationName;
			Feature = feature;
			Ip = ip;
			Url = url;
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

		public void AddEntities(params AuditEntity[] entities)
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
			if (!CreationTime.HasValue)
			{
				Elapsed = 0;
			}
			else
			{
				Elapsed = (int) (EndTime - CreationTime.Value).TotalMilliseconds;
			}
		}

		public override string ToString()
		{
			return
				$"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'ApplicationName': {ApplicationName}, 'Feature': {Feature}, 'Ip': {Ip}, 'UserAgent': {UserAgent}, 'EndedTime': {EndTime:yyyy-MM-dd HH:mm:ss}, 'Elapsed': {Elapsed} }}";
		}
	}
}