namespace MicroserviceFramework.Mediator
{
	public interface IMessage
	{
		/// <summary>
		/// 事件源标识
		/// </summary>
		string MessageId { get; }

		/// <summary>
		/// 事件发生时间
		/// </summary>
		long MessageTime { get; }
	}
}