using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
/// <see cref="IConfiguration"/> 扩展，从配置节点解析对应 <see cref="DbContextSettings"/>。
/// 支持两种 JSON 格式：
///   - 对象（推荐）: "DbContexts": { "MyApp.MyContext": { ... } }
///   - 数组（兼容）: "DbContexts": [{ "DbContextTypeName": "MyApp.MyContext", ... }]
/// </summary>
public static class DbContextSettingsExtensions
{
    private const string SectionName = "DbContexts";

    /// <summary>
    /// 从 <c>DbContexts</c> 配置节中获取指定 <typeparamref name="TContext"/> 对应的设置。
    /// 若配置中不包含该类型的条目则抛出 <see cref="InvalidOperationException"/>。
    /// </summary>
    public static DbContextSettings GetDbContextSettings<TContext>(this IConfiguration configuration)
        where TContext : DbContextBase
    {
        var typeName = typeof(TContext).FullName!;
        var section = configuration.GetSection(SectionName);

        // 优先：字典格式（Key = 类型全名）
        var dict = section.Get<Dictionary<string, DbContextSettings>>();
        if (dict != null && dict.TryGetValue(typeName, out var settings))
        {
            settings.DbContextTypeName = typeName;
            return settings;
        }

        // 兼容：列表格式
        var list = section.Get<List<DbContextSettings>>();
        if (list is { Count: > 0 })
        {
            var match = list.FirstOrDefault(x => Type.GetType(x.DbContextTypeName) == typeof(TContext));
            if (match != null)
            {
                return match;
            }

            // 列表中只有一个且未指定 TypeName，视为默认配置
            if (list.Count == 1 && string.IsNullOrEmpty(list[0].DbContextTypeName))
            {
                list[0].DbContextTypeName = typeName;
                return list[0];
            }
        }

        throw new InvalidOperationException(
            $"未找到 {typeName} 的数据库配置。请在 appsettings.json 的 \"{SectionName}\" 节中添加。");
    }
}
