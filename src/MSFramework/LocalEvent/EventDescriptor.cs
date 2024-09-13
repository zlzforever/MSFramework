using System;
using System.Reflection;

namespace MicroserviceFramework.LocalEvent;

internal record EventDescriptor(Type HandlerType, MethodInfo HandleMethod);
