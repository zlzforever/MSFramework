using System;
using System.Reflection;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 事件处理器描述符，包含处理器类型和 HandleAsync 方法信息。
/// </summary>
/// <param name="HandlerType">事件处理器类型</param>
/// <param name="HandleMethod">HandleAsync 方法反射信息</param>
public record EventDescriptor(Type HandlerType, MethodInfo HandleMethod);
