using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Mediator
{
	public class MessageBase : IMessage
	{
		public string MessageId { get; }
		public long MessageTime { get; }

		protected MessageBase()
		{
			MessageId = ObjectId.GenerateNewId().ToString();
			MessageTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}

		public override string ToString()
		{
			return
				$"[{GetType().Name}] EventId = {MessageId}, EventTime = {DateTimeOffset.FromUnixTimeMilliseconds(MessageTime):YYYY-MM-DD HH:mm:ss}";
		}
	}
}