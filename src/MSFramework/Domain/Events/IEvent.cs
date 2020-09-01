using System;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Domain.Events
{
	public interface IEvent
	{
		/// <summary>
		/// 事件标识
		/// </summary>
		ObjectId EventId { get; }

		/// <summary>
		/// 事件发生时间
		/// </summary>
		DateTimeOffset EventTime { get; }
	}
}