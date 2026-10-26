namespace System.Runtime.CompilerServices;

/// <summary>
/// netstandard2.0 下 init 访问器所需的类型垫片：
/// record 位置属性与 init setter 编译为 <c>modreq(IsExternalInit)</c>，
/// 该类型在 netstandard2.0 中未内置，需手动定义（源生成器项目标准做法）
/// </summary>
internal static class IsExternalInit;
