using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.Domain.Events
{
    /// <summary>
    /// Event used when an order is created
    /// </summary>
    public class OrderStartedDomainEvent : DomainEventBase<Guid>
    {
        public string UserId { get; }
        public string UserName { get; }
        public int CardTypeId { get; }
        public string CardNumber { get; }
        public string CardSecurityNumber { get; }
        public string CardHolderName { get; }
        public DateTime CardExpiration { get; }
        public Order Order { get; }

        public OrderStartedDomainEvent(Order order, string userId, string userName,
                                       int cardTypeId, string cardNumber, 
                                       string cardSecurityNumber, string cardHolderName, 
                                       DateTime cardExpiration)
        {
            Order = order;
            UserId = userId;
            UserName = userName;
            CardTypeId = cardTypeId;
            CardNumber = cardNumber;
            CardSecurityNumber = cardSecurityNumber;
            CardHolderName = cardHolderName;
            CardExpiration = cardExpiration;
        }
    }
}
