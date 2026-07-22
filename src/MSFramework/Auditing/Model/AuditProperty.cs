using System.Text.Json.Serialization;
using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace MicroserviceFramework.Auditing.Model;

/// <summary>
/// 实体属性审计信息
/// </summary>
public class AuditProperty : EntityBase<string>, IAuditObject
{
    private AuditProperty(string id) : base(id)
    {
    }

    /// <summary>
    /// 创建属性审计记录。
    /// </summary>
    /// <param name="name">属性名</param>
    /// <param name="type">属性类型</param>
    /// <param name="originalValue">原始值</param>
    /// <param name="newValue">新值</param>
    public AuditProperty(string name, string type, string originalValue, string newValue)
        : this(ObjectId.GenerateNewId().ToString())
    {
        Name = name;
        Type = type;
        OriginalValue = originalValue;
        NewValue = newValue;
    }

    /// <summary>
    /// 所属实体
    /// </summary>
    [JsonIgnore]
    public virtual AuditEntity Entity { get; internal set; }

    /// <summary>
    /// 字段
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// 旧值
    /// </summary>
    public string OriginalValue { get; private set; }

    /// <summary>
    /// 新值
    /// </summary>
    public string NewValue { get; private set; }

    /// <summary>
    /// 返回调试友好的字符串表示。
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'PropertyName': {Name}, 'PropertyType': {Type}, 'OriginalValue': {OriginalValue}, 'NewValue': {NewValue} }}";
    }
}
