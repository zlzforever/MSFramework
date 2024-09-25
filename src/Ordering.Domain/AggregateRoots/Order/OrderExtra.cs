namespace Ordering.Domain.AggregateRoots.Order;

public class OrderExtra(string key, string value)
{
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;
}
