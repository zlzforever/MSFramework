using System;
using System.Collections.Generic;
using MicroserviceFramework.EventBus;

namespace Ordering.Application.Events;

public record UserCheckoutAcceptedEvent(
    List<UserCheckoutAcceptedEvent.OrderItemDTO> basketItems,
    string userId,
    string city,
    string street,
    string state,
    string country,
    string zipCode,
    string description)
    : EventBase
{
    public class OrderItemDTO
    {
        public Guid ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public int Units { get; set; }

        public string PictureUrl { get; set; }
    }

    public string UserId { get; } = userId;

    public string City { get; set; } = city;

    public string Street { get; set; } = street;

    public string State { get; set; } = state;

    public string Country { get; set; } = country;

    public string ZipCode { get; set; } = zipCode;

    public string Description { get; } = description;

    public List<OrderItemDTO> OrderItems { get; set; } = basketItems;
}
