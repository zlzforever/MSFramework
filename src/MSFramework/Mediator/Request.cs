using MicroserviceFramework.Serialization;

namespace MicroserviceFramework.Mediator;

/// <summary>
/// 无返回值的请求基类。
/// </summary>
public abstract record Request;

/// <summary>
/// 带返回值的请求基类。
/// </summary>
/// <typeparam name="TResponse">响应类型</typeparam>
public abstract record Request<TResponse>
{
    /// <summary>
    /// 返回调试友好的字符串表示。
    /// </summary>
    public override string ToString()
    {
        return $"Response: {typeof(TResponse)}";
    }
}
