using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Mediator;

namespace Ordering.Application.Commands;

public record DeleteOrderCommand : Request
{
    /// <summary>
    ///
    /// </summary>
    [Required, StringLength(36)]
    public string OrderId { get; set; }
}
