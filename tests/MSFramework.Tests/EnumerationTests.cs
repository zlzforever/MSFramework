using System.Linq;
using MicroserviceFramework.Domain;
using Xunit;

namespace MSFramework.Tests;

public class OrderStatus(string id, string name) : Enumeration(id, name)
{
    public static OrderStatus Submitted = new OrderStatus(nameof(Submitted), nameof(Submitted));

    public static OrderStatus AwaitingValidation =
        new OrderStatus(nameof(AwaitingValidation), nameof(AwaitingValidation));

    public static OrderStatus StockConfirmed = new OrderStatus(nameof(StockConfirmed), nameof(StockConfirmed));
    public static OrderStatus Paid = new OrderStatus(nameof(Paid), nameof(Paid));
    public static OrderStatus Shipped = new OrderStatus(nameof(Shipped), nameof(Shipped));
    public static OrderStatus Cancelled = new OrderStatus(nameof(Cancelled), nameof(Cancelled));
}

public class EnumerationTests
{
    [Fact]
    public void Parse()
    {
        var stockConfirmed = Enumeration.Parse(typeof(OrderStatus), "StockConfirmed");
        Assert.Equal(OrderStatus.StockConfirmed, stockConfirmed);
        Assert.Equal(OrderStatus.Cancelled, Enumeration.FromValue<OrderStatus>("Cancelled"));
        Assert.Equal(OrderStatus.Cancelled, Enumeration.FromDisplayName<OrderStatus>("Cancelled"));
    }

    [Fact]
    public void GetAll()
    {
        var list = Enumeration.GetAll(typeof(OrderStatus)).ToList();
        Assert.Equal(6, list.Count);
        var list2 = Enumeration.GetAll<OrderStatus>().ToList();
        Assert.Equal(6, list2.Count);
    }

    [Fact]
    public void Equal()
    {
        var e1 = OrderStatus.Cancelled;
        var e2 = OrderStatus.Cancelled;
        var r = e1 == e2;
        Assert.True(r);
    }

    [Fact]
    public void NotEqual()
    {
        var e1 = OrderStatus.Cancelled;
        var e2 = OrderStatus.StockConfirmed;
        var r = e1 != e2;
        Assert.True(r);
    }
}
