using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Order;

public class OrderStatus : Enumeration
{
    public static readonly OrderStatus Submitted = new(nameof(Submitted), nameof(Submitted));
    public static readonly OrderStatus AwaitingValidation = new(nameof(AwaitingValidation), nameof(AwaitingValidation));
    public static readonly OrderStatus StockConfirmed = new(nameof(StockConfirmed), nameof(StockConfirmed));
    public static readonly OrderStatus Paid = new(nameof(Paid), nameof(Paid));
    public static readonly OrderStatus Shipped = new(nameof(Shipped), nameof(Shipped));
    public static readonly OrderStatus Cancelled = new(nameof(Cancelled), nameof(Cancelled));

    private OrderStatus(string id, string name) : base(id, name)
    {
    }
}
