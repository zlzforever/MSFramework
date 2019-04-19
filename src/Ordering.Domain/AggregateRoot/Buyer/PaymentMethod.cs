using System;
using MSFramework.Domain.Entity;

namespace Ordering.Domain.AggregateRoot.Buyer
{
	public class PaymentMethod
		: EntityBase<Guid>
	{
		private string _alias;
		private string _cardNumber;
		private string _securityNumber;
		private string _cardHolderName;
		private DateTime _expiration;

		private int _cardTypeId;

		public CardType CardType => CardType.From(_cardTypeId);

		protected PaymentMethod()
		{
		}

		public PaymentMethod(int cardTypeId, string alias, string cardNumber, string securityNumber,
			string cardHolderName, DateTime expiration)
		{
			_cardNumber = !string.IsNullOrWhiteSpace(cardNumber)
				? cardNumber
				: throw new OrderingException(nameof(cardNumber));
			_securityNumber = !string.IsNullOrWhiteSpace(securityNumber)
				? securityNumber
				: throw new OrderingException(nameof(securityNumber));
			_cardHolderName = !string.IsNullOrWhiteSpace(cardHolderName)
				? cardHolderName
				: throw new OrderingException(nameof(cardHolderName));

			if (expiration < DateTime.UtcNow)
			{
				throw new OrderingException(nameof(expiration));
			}

			_alias = alias;
			_expiration = expiration;
			_cardTypeId = cardTypeId;
		}

		public bool IsEqual(int cardTypeId, string cardNumber, DateTime expiration)
		{
			return _cardTypeId == cardTypeId
			       && _cardNumber == cardNumber
			       && _expiration == expiration;
		}
	}
}