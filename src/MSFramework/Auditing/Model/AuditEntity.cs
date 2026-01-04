using System.Collections.Generic;
using System.Text.Json.Serialization;
using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace MicroserviceFramework.Auditing.Model;

/// <summary>
/// 实体审计信息
/// </summary>
public class AuditEntity : EntityBase<string>, IAuditObject
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="entityId"></param>
    /// <param name="operationType"></param>
    public AuditEntity(string typeName, string entityId, OperationType operationType) : this(ObjectId.GenerateNewId()
        .ToString())
    {
        Type = typeName;
        EntityId = entityId;
        OperationType = operationType;
    }

    private AuditEntity(string id) : base(id)
    {
        Properties = new List<AuditProperty>();
    }

    /// <summary>
    /// 所属的操作
    /// </summary>
    [JsonIgnore]
    public AuditOperation Operation { get; internal set; }

    /// <summary>
    /// 类型名称
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// 数据标识
    /// </summary>
    public string EntityId { get; private set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public OperationType OperationType { get; private set; }

    /// <summary>
    /// 操作实体属性集合
    /// </summary>
    public ICollection<AuditProperty> Properties { get; private set; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="properties"></param>
    public void AddProperties(IEnumerable<AuditProperty> properties)
    {
        foreach (var property in properties)
        {
            property.Entity = this;
            Properties.Add(property);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"Id = {Id}, TypeName = {Type}, EntityId = {EntityId}, OperateType = {OperationType}";
    }
}
