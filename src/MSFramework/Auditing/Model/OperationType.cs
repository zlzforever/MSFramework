using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Auditing;

/// <summary>
/// 表示实体审计操作类型
/// </summary>
public class OperationType : Enumeration
{
    public static OperationType Query = new(nameof(Query), nameof(Query));
    public static OperationType Add = new(nameof(Add), nameof(Add));
    public static OperationType Modify = new(nameof(Modify), nameof(Modify));
    public static OperationType Delete = new(nameof(Delete), nameof(Delete));

    private OperationType(string id, string name) : base(id, name)
    {
    }
}
