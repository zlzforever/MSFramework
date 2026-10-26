using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Template.Domain.Aggregates;

/// <summary>商品创建领域事件，由当前工作单元在保存前派发。</summary>
/// <param name="ProductId">新商品的离线标识。</param>
public record ProductCreatedEvent(ObjectId ProductId) : DomainEvent;
