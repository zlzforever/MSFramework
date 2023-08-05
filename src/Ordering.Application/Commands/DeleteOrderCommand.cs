using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Ordering.Application.Commands;

public record DeleteOrderCommand : Request
{
    /// <summary>
    ///
    /// </summary>
    [Required]
    public ObjectId OrderId { get; set; }
}
