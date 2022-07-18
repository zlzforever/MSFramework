using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Serialization.Newtonsoft;
using MicroserviceFramework.Serialization.Newtonsoft.Converters;
using MicroserviceFramework.Text.Json;
using MongoDB.Bson;
using Newtonsoft.Json;
using Xunit;

namespace MSFramework.Tests;

public class SerializationTests
{
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
    }

    public class Price2
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

        public Price2(decimal? value, Unit unit, Currency currency)
        {
            Value = value;
            Unit = unit;
            Currency = currency;
        }
    }

    private class Obj
    {
        public ObjectId Id { get; set; }
        public Enum1 Enum { get; set; }
    }

    private class Enum1 : Enumeration
    {
        public static Enum1 Graph = new Enum1(nameof(Graph), nameof(Graph));
        public static Enum1 Property = new Enum1(nameof(Property), nameof(Property));

        public Enum1(string id, string name) : base(id, name)
        {
        }
    }

    [Fact]
    public void EnumTest()
    {
        var obj = new Obj
        {
            Id = ObjectId.GenerateNewId(),
            Enum = Enum1.Graph
        };
        var jsonHelper = JsonHelper.Create();
        var json = jsonHelper.Serialize(new List<Obj>()
        {
            obj
        });
        var result = jsonHelper.Deserialize<List<Obj>>(json);
        Assert.Single(result);
        Assert.Equal(obj.Id, result[0].Id);
        Assert.Equal(obj.Enum, result[0].Enum);
    }

    [Fact]
    public void EnumPrivateSetterTest()
    {
        var json1 = @"
[
    {
        ""unit"": ""Hourly"",
			""value"": 2000.0,
			""currency"": ""CNY""
		}
		]";
        var jsonHelper = JsonHelper.Create();
        var result2 = jsonHelper.Deserialize<List<Price>>(json1);
        Assert.Single(result2);
        Assert.Equal(Currency.CNY, result2[0].Currency);
        Assert.Equal(Unit.Hourly, result2[0].Unit);
        Assert.Equal(2000.0m, result2[0].Value);
    }

    [Fact]
    public void EnumWithConstructorTest()
    {
        var json1 = @"
[
    {
        ""unit"": ""Hourly"",
			""value"": 2000.0,
			""currency"": ""CNY""
		}
		]";
        var jsonHelper = JsonHelper.Create();
        var result2 = jsonHelper.Deserialize<List<Price2>>(json1);
        Assert.Single(result2);
        Assert.Equal(Currency.CNY, result2[0].Currency);
        Assert.Equal(Unit.Hourly, result2[0].Unit);
        Assert.Equal(2000.0m, result2[0].Value);
    }

    [Fact]
    public void DatetimeOffsetTest()
    {
        var a = System.Text.Json.JsonSerializer.Serialize(DateTimeOffset.Now);
        var b = JsonConvert.SerializeObject(DateTimeOffset.Now);
        Assert.Contains("T", a);
        Assert.Contains("T", b);
    }

    [Fact]
    public void DeserializeNullToObjectId1()
    {
        var options = new JsonSerializerOptions();
        // options.Converters.Add(new ObjectIdJsonConverter());
        var serializer = new JsonHelper(options);

        serializer.Serialize(new { id = ObjectId.GenerateNewId() });

        var json = "{\"id\":null}";
        var obj = serializer.Deserialize<Obj>(json);

        Assert.Equal(ObjectId.Empty, obj.Id);

        json = "{\"id\":\"\"}";
        obj = serializer.Deserialize<Obj>(json);
        Assert.Equal(ObjectId.Empty, obj.Id);

        json = "{}";
        obj = serializer.Deserialize<Obj>(json);
        Assert.Equal(ObjectId.Empty, obj.Id);

        json = "{\"enum\":\"Graph\"}";
        obj = serializer.Deserialize<Obj>(json);
        Assert.Equal(ObjectId.Empty, obj.Id);
    }

    [Fact]
    public void DeserializeNullToObjectId2()
    {
        var options = new JsonSerializerSettings();
        options.Converters.Add(new ObjectIdConverter());
        options.Converters.Add(new EnumerationConverter());
        JsonConvert.DefaultSettings = () => options;
        var serializer = new NewtonsoftJsonHelper();

        serializer.Serialize(new { id = ObjectId.GenerateNewId() });

        var json = "{\"id\":null}";
        var obj = serializer.Deserialize<Obj>(json);

        Assert.Equal(ObjectId.Empty, obj.Id);

        json = "{\"id\":\"\"}";
        obj = serializer.Deserialize<Obj>(json);
        Assert.Equal(ObjectId.Empty, obj.Id);

        json = "{}";
        obj = serializer.Deserialize<Obj>(json);
        Assert.Equal(ObjectId.Empty, obj.Id);

        json = "{\"enum\":\"Graph\"}";
        obj = serializer.Deserialize<Obj>(json);
        Assert.Equal(ObjectId.Empty, obj.Id);
    }
}
