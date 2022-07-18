using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots;

public class OrderStatus : Enumeration
{
    public static OrderStatus Submitted = new OrderStatus(nameof(Submitted), nameof(Submitted));

    public static OrderStatus AwaitingValidation =
        new OrderStatus(nameof(AwaitingValidation), nameof(AwaitingValidation));

    public static OrderStatus StockConfirmed = new OrderStatus(nameof(StockConfirmed), nameof(StockConfirmed));
    public static OrderStatus Paid = new OrderStatus(nameof(Paid), nameof(Paid));
    public static OrderStatus Shipped = new OrderStatus(nameof(Shipped), nameof(Shipped));
    public static OrderStatus Cancelled = new OrderStatus(nameof(Cancelled), nameof(Cancelled));

    public OrderStatus(string id, string name) : base(id, name)
    {
    }
}
