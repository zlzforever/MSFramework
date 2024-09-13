using System.Collections.Generic;
using MicroserviceFramework.Mediator;

namespace Ordering.Application.Commands;

public record CreateOrderCommand(
    List<CreateOrderCommand.OrderItemDTO> OrderItems,
    string UserId,
    string City,
    string Street,
    string State,
    string Country,
    string ZipCode,
    string Description)
    : Request<string>
{
    public string UserId { get; set; } = UserId;

    public string City { get; set; } = City;

    public string Street { get; set; } = Street;

    public string State { get; set; } = State;

    public string Country { get; set; } = Country;

    public string ZipCode { get; set; } = ZipCode;

    public string Description { get; set; } = Description;


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
