using MongoDB.Bson;
using Template.Domain.Aggregates;

namespace Template.Application.Products;

/// <summary>商品只读输出，避免向客户端暴露 EF 实体及导航属性。</summary>
/// <param name="Id">商品标识，JSON 表示为 24 位十六进制字符串。</param>
/// <param name="Name">商品名称。</param>
/// <param name="Price">商品价格，单位为分。</param>
/// <param name="ProductType">商品类别，JSON 表示为枚举 Id。</param>
/// <param name="CreationTime">创建时间，未提交的新对象可能为空。</param>
public sealed record ProductOut(ObjectId Id, string Name, int Price, ProductType ProductType,
    DateTimeOffset? CreationTime);
