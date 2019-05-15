using System;

namespace MSFramework.EventBus
{
	public interface IEvent
	{
		/// <summary>
		/// 事件源标识
		/// </summary>
		Guid Id { get; set; }

		/// <summary>
		/// 事件版本
		/// </summary>
		int Version { get; set; }

		/// <summary>
		/// 事件发生时间
		/// </summary>
		DateTimeOffset Timestamp { get; set; }

		/// <summary>
		/// 事件触发人员
		/// </summary>
		string Creator { get; set; }

		/// <summary>
		/// 事件触发人员标识
		/// </summary>
		string CreatorId { get; set; }
	}
}