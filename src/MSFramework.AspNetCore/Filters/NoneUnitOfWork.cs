using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NoneUnitOfWork : Attribute
{
}
