using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Common;
using MicroserviceFramework.Mediator;

namespace Template.Application.Products.Queries;

/// <summary>商品分页请求，由 API 查询字符串绑定，不使用路由绑定。</summary>
public record PagedProductQuery : Request<PaginationResult<ProductOut>>
{
    /// <summary>从 1 开始的页码。</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    /// <summary>每页数量，范围 1 至 100。</summary>
    [Range(1, 100)]
    public int Limit { get; init; } = 20;

    /// <summary>可选名称关键词，最长 100 字符。</summary>
    [StringLength(100)]
    public string? Keyword { get; init; }
}
