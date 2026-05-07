namespace MicroserviceFramework.Domain;

/// <summary>
/// 值类型
/// </summary>
/// <summary>
/// 值对象标记基类。
/// C# record 已提供完整的值语义（Equals/GetHashCode/ToString/with），
/// 此基类仅用于框架类型识别和公共行为扩展。
/// </summary>
public abstract record ValueObject;
