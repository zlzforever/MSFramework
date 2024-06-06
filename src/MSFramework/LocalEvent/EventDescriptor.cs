using System;
using System.Reflection;

namespace MicroserviceFramework.LocalEvent;

public record EventDescriptor(Type HandlerType, MethodInfo HandleMethod);
