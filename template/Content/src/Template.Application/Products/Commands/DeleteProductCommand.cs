using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Template.Application.Products.Commands;

/// <summary>按路由标识软删除商品，不接受来自请求体的替代标识。</summary>
/// <param name="Id">待删除商品标识。</param>
public record DeleteProductCommand(ObjectId Id) : Request;
