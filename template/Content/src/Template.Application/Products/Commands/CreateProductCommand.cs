using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Mediator;
using MongoDB.Bson;
using Template.Domain.Aggregates;

namespace Template.Application.Products.Commands;

/// <summary>商品创建输入，使用领域类别和分单位价格，不包含客户端指定的审计信息。</summary>
public record CreateProductCommand : Request<ObjectId>
{
    /// <summary>商品名称，去除首尾空白后为 1 至 100 字符。</summary>
    [Required, StringLength(100)]
    public string Name { get; init; } = string.Empty;

    /// <summary>商品价格，单位为分，不能为负。</summary>
    [Range(0, int.MaxValue)]
    public int Price { get; init; }

    /// <summary>商品类别，必须为枚举中定义的值。</summary>
    [Required]
    public ProductType ProductType { get; init; } = ProductType.Home;
}
