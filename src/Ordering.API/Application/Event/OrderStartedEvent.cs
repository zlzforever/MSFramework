namespace Ordering.API.Application.Event
{
	public class OrderStartedEvent : MSFramework.EventBus.Event
	{
		public string UserId { get; }

		public OrderStartedEvent(string userId)
			=> UserId = userId;
	}
}