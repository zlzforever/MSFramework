using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Ordering.Application.Commands;

public class DeleteOrderCommand : IRequest
{
    /// <summary>
    ///
    /// </summary>
    [Required]
    public ObjectId OrderId { get; set; }
}
