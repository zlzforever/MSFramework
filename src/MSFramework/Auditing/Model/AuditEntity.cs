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
    /// 创建审计实体记录。
    /// </summary>
    /// <param name="typeName">实体类型全名</param>
    /// <param name="entityId">实体标识</param>
    /// <param name="operationType">操作类型</param>
    public AuditEntity(string typeName, string entityId, OperationType operationType) : this(
        ObjectId.GenerateNewId()
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
    public AuditOperation Operation { get; private set; }

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
    /// 批量添加属性变更记录。
    /// </summary>
    /// <param name="properties">属性变更集合，为 null 时直接返回（审计链路处于后台静默执行流，
    /// 集合为 null 属可忽略的空输入，不应 NRE 导致整段审计丢失）；集合内元素为 null 时跳过</param>
    public void AddProperties(IEnumerable<AuditProperty> properties)
    {
        if (properties == null)
        {
            return;
        }

        foreach (var property in properties)
        {
            if (property == null)
            {
                continue;
            }

            property.Entity = this;
            Properties.Add(property);
        }
    }

    /// <summary>
    /// 关联所属的审计操作。
    /// </summary>
    /// <param name="operation">审计操作实例</param>
    public void SetOperation(AuditOperation operation)
    {
        Operation = operation;
    }

    /// <summary>
    /// 返回调试友好的字符串表示。
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"Id = {Id}, TypeName = {Type}, EntityId = {EntityId}, OperateType = {OperationType}";
    }
}
