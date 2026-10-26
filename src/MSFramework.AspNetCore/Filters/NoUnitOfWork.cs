using System;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// No 前缀明确表示"否定/不启用"：框架的工作单元提交是全局过滤器（UnitOfWork.cs）按
/// HTTP 写方法自动执行，本 Attribute 用于按 Controller/Action 跳过提交（不存在
/// UnitOfWorkAttribute，不要按旧文档去找它）
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NoUnitOfWork : Attribute;
