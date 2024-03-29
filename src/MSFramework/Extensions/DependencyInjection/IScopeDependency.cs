using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Extensions.DependencyInjection;

/// <summary>
/// 实现此接口的类型将被注册为<see cref="ServiceLifetime.Scoped"/>模式
/// </summary>
public interface IScopeDependency;
