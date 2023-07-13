namespace Ordering.Domain.AggregateRoots;

public class OrderExtra
{
    public string Key { get; set; }
    public string Value { get; set; }

    public OrderExtra(string key, string value)
    {
        Key = key;
        Value = value;
    }
}
