using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Auditing.Model;

/// <summary>
/// 表示实体审计操作类型
/// </summary>
public class OperationType : Enumeration
{
    /// <summary>
    /// 查询
    /// </summary>
    public static readonly OperationType Query = new(nameof(Query), nameof(Query));

    /// <summary>
    /// 创建
    /// </summary>
    public static readonly OperationType Add = new(nameof(Add), nameof(Add));

    /// <summary>
    /// 修改
    /// </summary>
    public static readonly OperationType Modify = new(nameof(Modify), nameof(Modify));

    /// <summary>
    /// 物理删除
    /// </summary>
    public static readonly OperationType Delete = new(nameof(Delete), nameof(Delete));

    /// <summary>
    /// 转删除
    /// </summary>
    public static readonly OperationType Remove = new(nameof(Remove), nameof(Remove));

    private OperationType(string id, string name) : base(id, name)
    {
    }
}
