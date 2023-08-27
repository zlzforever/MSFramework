using System.Collections.Generic;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.Dto;

public class OrderDto
{
    public string Id { get; set; }
    /// <summary>
    /// Address is a Value Object pattern example persisted as EF Core 2.0 owned entity
    /// </summary>
    public Address Address { get; set; }

    /// <summary>
    /// 订单项
    /// </summary>
    public List<OrderItemDto> Items { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// 购买人员
    /// </summary>
    public string BuyerId { get; set; }

    public string Description { get; set; }

    /// <summary>
    /// 测试 List 的 JSON
    /// </summary>
    public List<string> ListJson { get; set; }

    /// <summary>
    /// 测试对象列表的 JSON 存储
    /// </summary>
    public List<OrderExtraDto> Extras { get; set; }

    /// <summary>
    /// 用于测试字典 JSON 在 EF 中的序列化与反序列化
    /// KEY 的大小写差异
    /// </summary>
    public Dictionary<string, string> DictJson { get; set; }

    public class OrderExtraDto
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
