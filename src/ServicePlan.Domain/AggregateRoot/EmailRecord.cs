using System;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	/// <summary>
	/// 邮件发送服务记录
	/// </summary>
	public class EmailRecord : EntityBase<Guid>
	{
		/// <summary>
		/// 创建时间
		/// </summary>
		private DateTime _creationTime;

		private bool _success;

		/// <summary>
		/// 是否发送成功
		/// </summary>
		public bool Success => _success;

		/// <summary>
		/// 邮件响应时间
		/// </summary>
		public DateTime ResponseTime { get; private set; }

		/// <summary>
		/// 客户联系人
		/// </summary>
		public ClientUser ClientUser { get; }

		public EmailRecord(ClientUser clientUser, DateTime creationTime, Guid identity) : base(identity)
		{
			ClientUser = clientUser;
			_creationTime = creationTime;
		}

		public void SetSuccess(DateTime responseTime)
		{
			ResponseTime = responseTime;
			_success = true;
		}

		public void SetFailed(DateTime responseTime)
		{
			ResponseTime = responseTime;
			_success = false;
		}
	}
}