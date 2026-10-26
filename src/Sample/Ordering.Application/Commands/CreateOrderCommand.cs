using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Mediator;

namespace Ordering.Application.Commands;

/// <summary>
/// 下单输入，仅接受商品与数量及配送信息；身份来自会话，金额来自服务端商品目录。
/// </summary>
public record CreateOrderCommand : Request<string>
{
    /// <summary>购买明细，必须包含 1 至 100 行。</summary>
    [Required, MinLength(1), MaxLength(100)]
    public List<OrderItemDTO> OrderItems { get; init; } = [];

    /// <summary>配送城市，必填且最长 100 字符。</summary>
    [Required, StringLength(100)]
    public string City { get; init; }

    /// <summary>配送街道，必填且最长 255 字符。</summary>
    [Required, StringLength(255)]
    public string Street { get; init; }

    /// <summary>省或州，最长 100 字符。</summary>
    [StringLength(100)]
    public string State { get; init; }

    /// <summary>配送国家，必填且最长 100 字符。</summary>
    [Required, StringLength(100)]
    public string Country { get; init; }

    /// <summary>邮编，最长 20 字符。</summary>
    [StringLength(20)]
    public string ZipCode { get; init; }

    /// <summary>订单备注，最长 500 字符。</summary>
    [StringLength(500)]
    public string Description { get; init; }

    /// <summary>客户端购买意向，不包含任何权威价格、折扣或商品名称。</summary>
    public sealed record OrderItemDTO
    {
        /// <summary>商品目录中的标识，必填且最长 36 字符。</summary>
        [Required, StringLength(36)]
        public string ProductId { get; init; }

        /// <summary>购买数量，必须为正整数。</summary>
        [Range(1, int.MaxValue)]
        public int Units { get; init; }
    }
}
