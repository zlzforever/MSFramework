using System;
using MSFramework.Domain;

namespace Ordering.Domain.Events
{
	using MediatR;

	/// <summary>
	/// Event used when the order stock items are confirmed
	/// </summary>
	public class OrderStatusChangedToStockConfirmedDomainEvent
		: DomainEventBase<Guid>
	{
	}
}