using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Template.Application.Products.Events;

/// <summary>商品提交后的跨服务通知，接收方必须按业务标识实现幂等。</summary>
/// <param name="ProductId">已提交的商品标识。</param>
public record ProductCreatedIntegrationEvent(ObjectId ProductId) : Request;
