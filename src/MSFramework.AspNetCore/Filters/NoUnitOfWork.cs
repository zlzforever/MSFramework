using System;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// No前缀明确表示 “否定 / 不启用”，直接对应UnitOfWorkAttribute的反向功能
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NoUnitOfWork : Attribute;
