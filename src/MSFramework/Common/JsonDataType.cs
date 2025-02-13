// ReSharper disable InconsistentNaming
namespace MicroserviceFramework.Common;

/// <summary>
/// JSON 数据格式
/// </summary>
public enum JsonDataType
{
    /// <summary>
    /// 普通 JSON
    /// </summary>
    JSON,

    /// <summary>
    /// JSONB 是 JSON 的一种二进制存储格式，它将 JSON 数据转换为一种更紧凑、更高效的二进制表示形式
    /// 以便在数据库中进行存储和操作。与普通的 JSON 格式相比，JSONB 在存储和查询性能上有一定的优势。
    /// </summary>
    JSONB
}
