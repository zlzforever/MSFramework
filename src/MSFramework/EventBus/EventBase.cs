using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace MicroserviceFramework.EventBus
{
	public abstract class EventBase
	{
		/// <summary>
		/// 事件源标识
		/// </summary>
		public string EventId { get; set; }

		/// <summary>
		/// 事件发生时间
		/// </summary>
		public long EventTime { get; set; }

		protected EventBase()
		{
			EventId = ObjectId.GenerateNewId().ToString();
			EventTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}

		public override string ToString()
		{
			return
				$"[{GetType().Name}] EventId = {EventId}, EventTime = {DateTimeOffset.FromUnixTimeMilliseconds(EventTime):YYYY-MM-DD HH:mm:ss}";
		}
	}
}