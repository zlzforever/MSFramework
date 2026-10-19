using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Mediator;

namespace Ordering.Application.Commands;

public record DeleteOrderItemCommand : Request
{
    /// <summary>
    /// 所属订单标识
    /// </summary>
    [Required, StringLength(36)]
    public string OrderId { get; set; }

    /// <summary>
    /// 订单项标识
    /// </summary>
    [Required, StringLength(36)]
    public string OrderItemId { get; set; }
}
