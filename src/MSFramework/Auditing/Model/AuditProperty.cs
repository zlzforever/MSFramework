using System.Text.Json.Serialization;
using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace MicroserviceFramework.Auditing.Model;

/// <summary>
/// 实体属性审计信息
/// </summary>
public class AuditProperty : EntityBase<ObjectId>
{
    private AuditProperty() : base(ObjectId.GenerateNewId())
    {
    }

    public AuditProperty(string propertyName, string propertyType, string originalValue, string newValue)
        : this()
    {
        Name = propertyName;
        Type = propertyType;
        OriginalValue = originalValue;
        NewValue = newValue;
    }

    /// <summary>
    /// 所属实体
    /// </summary>
    [JsonIgnore]
    public virtual AuditEntity Entity { get; internal set; }

    /// <summary>
    /// 获取或设置 字段
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 获取或设置 数据类型
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// 获取或设置 旧值
    /// </summary>
    public string OriginalValue { get; private set; }

    /// <summary>
    /// 获取或设置 新值
    /// </summary>
    public string NewValue { get; private set; }

    public override string ToString()
    {
        return
            $"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'PropertyName': {Name}, 'PropertyType': {Type}, 'OriginalValue': {OriginalValue}, 'NewValue': {NewValue} }}";
    }
}
