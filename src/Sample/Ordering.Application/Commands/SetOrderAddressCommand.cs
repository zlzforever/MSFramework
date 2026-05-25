using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Mediator;
using Ordering.Domain.AggregateRoots.Order;

namespace Ordering.Application.Commands;

public record SetOrderAddressCommand : Request
{
    public Address Address { get; set; }

    /// <summary>
    ///
    /// </summary>
    [StringLength(36)]
    public string OrderId { get; set; }
}
