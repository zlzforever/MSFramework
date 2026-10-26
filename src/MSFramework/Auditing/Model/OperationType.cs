using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Auditing.Model;

/// <summary>
/// 表示实体审计操作类型
/// </summary>
/// <remarks>
/// <see cref="Enumeration"/> 的子类约定要求具体枚举类型 sealed 且只包含只读成员：
/// FromValue/Parse 与 EF 物化转换器返回的都是静态字段上的共享实例，
/// 未 sealed 时可在共享实例上追加派生可变状态，一处修改会污染所有持有该枚举值的实体
/// </remarks>
public sealed class OperationType : Enumeration
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
