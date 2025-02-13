using System;
using System.Reflection;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
///
/// </summary>
/// <param name="HandlerType"></param>
/// <param name="HandleMethod"></param>
public record EventDescriptor(Type HandlerType, MethodInfo HandleMethod);
