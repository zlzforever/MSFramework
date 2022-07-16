// See https://aka.ms/new-console-template for more information

using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DeepCopy;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Serialization;

namespace Benchmark;

public class Currency
    : Enumeration
{
    public Currency(string id, string name) : base(id, name)
    {
    }

    /// <summary>
    /// RMB
    /// </summary>
    public static readonly Currency CNY = new Currency(nameof(CNY), nameof(CNY));

    /// <summary>
    /// 小时数
    /// </summary>
    public static readonly Currency Hour = new Currency(nameof(Hour), nameof(Hour));

    /// <summary>
    /// 美元
    /// </summary>
    public static readonly Currency USD = new Currency(nameof(USD), nameof(USD));
}

public class Unit : Enumeration
{
    /// <summary>
    /// 小时
    /// </summary>
    public static readonly Unit Hourly = new Unit(nameof(Hourly), nameof(Hourly));

    /// <summary>
    /// 场
    /// </summary>
    public static readonly Unit Event = new Unit(nameof(Event), nameof(Event));

    /// <summary>
    /// 包
    /// </summary>
    public static readonly Unit Package = new Unit(nameof(Package), nameof(Package));

    /// <summary>
    /// 天
    /// </summary>
    public static readonly Unit Daily = new Unit(nameof(Daily), nameof(Daily));

    /// <summary>
    /// 个
    /// </summary>
    public static readonly Unit Per = new Unit(nameof(Per), nameof(Per));

    public Unit(string id, string name) : base(id, name)
    {
    }
}

public class Price
{
    /// <summary>
    /// 费率
    /// </summary>
    [JsonInclude]
    public decimal? Value { get; private set; }

    /// <summary>
    /// 费率单位
    /// </summary>
    [JsonInclude]
    public Unit Unit { get; private set; }

    /// <summary>
    /// 币种
    /// </summary>
    [JsonInclude]
    public Currency Currency { get; private set; }

    public Price(decimal? value, Unit unit, Currency currency)
    {
        Value = value;
        Unit = unit;
        Currency = currency;
    }
}

public class MyClass
{
    private static readonly int Count = 1000;
    private static readonly IJsonHelper JsonHelper = MicroserviceFramework.Text.Json.JsonHelper.Create();
    private static readonly Price Price = new Price(2000.0m, Unit.Hourly, Currency.CNY);

    [Benchmark]
    public void SerializeThenDeserialize()
    {
        for (var i = 0; i < Count; i++)
        {
            JsonHelper.Deserialize<Price>(JsonHelper.Serialize(Price));
        }
    }

    [Benchmark]
    public void DeepCopy()
    {
        for (var i = 0; i < Count; i++)
        {
            DeepCopier.Copy(Price);
        }
    }
}

internal class Prograom
{
    public static void Main()
    {
        BenchmarkRunner.Run<MyClass>();
        Console.WriteLine("Bye!");
    }
}