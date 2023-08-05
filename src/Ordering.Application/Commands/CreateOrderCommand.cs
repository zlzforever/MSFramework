using System.Collections.Generic;
using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Ordering.Application.Commands;

public record CreateOrderCommand : Request<ObjectId>
{
    public string UserId { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    public string State { get; set; }

    public string Country { get; set; }

    public string ZipCode { get; set; }

    public string Description { get; set; }

    public List<OrderItemDTO> OrderItems { get; }


    public CreateOrderCommand(List<OrderItemDTO> basketItems, string userId, string city,
        string street, string state, string country, string zipcode, string description)
    {
        OrderItems = basketItems;
        UserId = userId;

        City = city;
        Street = street;
        State = state;
        Country = country;
        ZipCode = zipcode;
        Description = description;
    }

    public class OrderItemDTO
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public int Units { get; set; }

        public string PictureUrl { get; set; }
    }
}
