using System;
using MediatR;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoot.Buyer;

namespace Ordering.Domain.Events
{
	public class BuyerAndPaymentMethodVerifiedDomainEvent
		: DomainEventBase<Guid>
	{
		public Buyer Buyer { get; private set; }
		
		public PaymentMethod Payment { get; private set; }

		public Guid OrderId { get; }

		public BuyerAndPaymentMethodVerifiedDomainEvent(Buyer buyer, PaymentMethod payment, Guid orderId)
		{
			Buyer = buyer;
			Payment = payment;
			OrderId = orderId;
		}
	}
}