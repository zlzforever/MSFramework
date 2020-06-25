using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace MSFramework.Extensions
{
	/// <summary>
	/// <see cref="ClaimsIdentity"/>扩展操作类
	/// </summary>
	public static class ClaimsIdentityExtensions
	{
		/// <summary>
		/// 获取用户ID
		/// </summary>
		public static string GetUserId(this IIdentity identity)
		{
			if (identity == null || !(identity is ClaimsIdentity claimsIdentity))
			{
				return null;
			}

			return claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		}

		/// <summary>
		/// 获取用户名
		/// </summary>
		public static string GetUserName(this IIdentity identity)
		{
			if (identity == null || !(identity is ClaimsIdentity claimsIdentity))
			{
				return null;
			}

			return claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
		}

		/// <summary>
		/// 获取Email
		/// </summary>
		public static string GetEmail(this IIdentity identity)
		{
			if (identity == null || !(identity is ClaimsIdentity claimsIdentity))
			{
				return null;
			}

			return claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
		}
	}
}