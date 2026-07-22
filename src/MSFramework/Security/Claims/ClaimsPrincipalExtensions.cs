using System.Linq;
using System.Security.Claims;
using MicroserviceFramework.Runtime;

namespace MicroserviceFramework.Security.Claims;

/// <summary>
/// <see cref="ClaimsIdentity"/>扩展操作类
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// 从 <see cref="ClaimsPrincipal"/> 按顺序查找第一个非空 Claim 值。
    /// </summary>
    /// <param name="principal">用户声明主体</param>
    /// <param name="claims">要查找的 Claim 类型（按优先级排序）</param>
    /// <returns>第一个非空的 Claim 值，未找到时返回 null</returns>
    public static string GetValue(this ClaimsPrincipal principal, params string[] claims)
    {
        if (principal == null)
        {
            return null;
        }

        return claims.Select(claim => principal.FindFirst(claim)?.Value)
            .FirstOrDefault(value => !value.IsNullOrEmpty());
    }

    /// <summary>
    /// 获取用户ID
    /// </summary>
    public static string GetUserId(this ClaimsIdentity claimsIdentity)
    {
        return claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
