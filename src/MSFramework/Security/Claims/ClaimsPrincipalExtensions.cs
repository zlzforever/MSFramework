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
    ///
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="claims"></param>
    /// <returns></returns>
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
