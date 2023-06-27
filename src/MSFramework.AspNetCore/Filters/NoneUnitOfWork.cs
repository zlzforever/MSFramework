using System;

namespace MicroserviceFramework.AspNetCore.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NoneUnitOfWork : Attribute
{
}
