using System;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
///
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipUnitOfWork : Attribute;
