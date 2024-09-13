using System.Collections.Generic;
using System.Text.Json.Serialization;
using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace MicroserviceFramework.Auditing.Model;

/// <summary>
///
/// </summary>
public class AuditEntity : EntityBase<string>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="entityId"></param>
    /// <param name="operationType"></param>
    public AuditEntity(string typeName, string entityId, OperationType operationType) : this()
    {
        Type = typeName;
        EntityId = entityId;
        OperationType = operationType;
    }

    private AuditEntity() : base(ObjectId.GenerateNewId().ToString())
    {
        Properties = new List<AuditProperty>();
    }

    /// <summary>
    /// 所属的操作
    /// </summary>
    [JsonIgnore]
    public AuditOperation Operation { get; internal set; }

    /// <summary>
    /// 获取或设置 类型名称
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// 获取或设置 数据编号
    /// </summary>
    public string EntityId { get; private set; }

    /// <summary>
    /// 获取或设置 操作类型
    /// </summary>
    public OperationType OperationType { get; private set; }

    /// <summary>
    /// 获取或设置 操作实体属性集合
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
