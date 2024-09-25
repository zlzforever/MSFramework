using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Order;

public class OrderStatus : Enumeration
{
    public static OrderStatus Submitted = new(nameof(Submitted), nameof(Submitted));

    public static OrderStatus AwaitingValidation = new(nameof(AwaitingValidation), nameof(AwaitingValidation));

    public static OrderStatus StockConfirmed = new(nameof(StockConfirmed), nameof(StockConfirmed));
    public static OrderStatus Paid = new(nameof(Paid), nameof(Paid));
    public static OrderStatus Shipped = new(nameof(Shipped), nameof(Shipped));
    public static OrderStatus Cancelled = new(nameof(Cancelled), nameof(Cancelled));

    private OrderStatus(string id, string name) : base(id, name)
    {
    }
}
