using System.Linq;
using System.Security.Claims;

namespace MicroserviceFramework.Extensions
{
	/// <summary>
	/// <see cref="ClaimsIdentity"/>扩展操作类
	/// </summary>
	public static class ClaimsPrincipalExtensions
	{
		public static string GetValue(this ClaimsPrincipal principal, params string[] claims)
		{
			if (principal == null)
			{
				return null;
			}

			return claims.Select(claim => principal.FindFirst(claim)?.Value)
				.FirstOrDefault(value => !value.IsNullOrWhiteSpace());
		}

		/// <summary>
		/// 获取用户ID
		/// </summary>
		public static string GetUserId(this ClaimsIdentity claimsIdentity)
		{
			return claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		}

		/// <summary>
		/// 获取用户名
		/// </summary>
		public static string GetUserName(this ClaimsIdentity claimsIdentity)
		{
			return claimsIdentity?.FindFirst(ClaimTypes.Name)?.Value;
		}

		/// <summary>
		/// 获取Email
		/// </summary>
		public static string GetEmail(this ClaimsIdentity claimsIdentity)
		{
			return claimsIdentity?.FindFirst(ClaimTypes.Email)?.Value;
		}
	}
}