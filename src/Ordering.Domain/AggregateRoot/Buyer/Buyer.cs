using System;
using System.Collections.Generic;
using System.Linq;
using MSFramework.Domain;
using Ordering.Domain.Events;

namespace Ordering.Domain.AggregateRoot.Buyer
{
	public class Buyer : AggregateRootBase<Guid>
	{
		private List<PaymentMethod> _paymentMethods;

		public string Identity { get; private set; }

		public string Name { get; private set; }

		public IEnumerable<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

		protected Buyer()
		{
			_paymentMethods = new List<PaymentMethod>();
		}

		public Buyer(string identity, string name) : this()
		{
			Identity = !string.IsNullOrWhiteSpace(identity)
				? identity
				: throw new ArgumentNullException(nameof(identity));
			Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));
		}

		public PaymentMethod AddPaymentMethod(
			int cardTypeId,
			string alias,
			string cardNumber,
			string securityNumber,
			string cardHolderName,
			DateTime expiration,
			Guid orderId)
		{
			var existingPayment = _paymentMethods
				.SingleOrDefault(p => p.IsEqual(cardTypeId, cardNumber, expiration));

			if (existingPayment != null)
			{
				RegisterDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, existingPayment, orderId));

				return existingPayment;
			}

			var payment = new PaymentMethod(cardTypeId, alias, cardNumber, securityNumber, cardHolderName, expiration);

			_paymentMethods.Add(payment);

			RegisterDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, payment, orderId));

			return payment;
		}
	}
}