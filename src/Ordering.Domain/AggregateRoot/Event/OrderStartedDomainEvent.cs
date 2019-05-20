using MediatR;

namespace Ordering.Domain.AggregateRoot.Event
{
	public class OrderStartedDomainEvent : INotification
	{
		public string UserId { get; }

		public Order Order { get; }

		public OrderStartedDomainEvent(Order order, string userId )
		{
			Order = order;
			UserId = userId;
		}
	}
}