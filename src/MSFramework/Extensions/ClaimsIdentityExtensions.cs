using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using MSFramework.Data;

namespace MSFramework.Extensions
{
	/// <summary>
	/// <see cref="ClaimsIdentity"/>扩展操作类
	/// </summary>
	public static class ClaimsIdentityExtensions
	{
		/// <summary>
		/// 获取指定类型的Claim值
		/// </summary>
		public static string GetClaimValueFirstOrDefault(this IIdentity identity, string type)
		{
			identity.NotNull(nameof(identity));
			if (!(identity is ClaimsIdentity claimsIdentity))
			{
				return null;
			}

			return claimsIdentity.FindFirst(type)?.Value;
		}

		/// <summary>
		/// 获取指定类型的所有Claim值
		/// </summary>
		public static string[] GetClaimValues(this IIdentity identity, string type)
		{
			identity.NotNull(nameof(identity));
			if (!(identity is ClaimsIdentity claimsIdentity))
			{
				return null;
			}

			return claimsIdentity.Claims.Where(m => m.Type == type).Select(m => m.Value).ToArray();
		}

		/// <summary>
		/// 获取用户ID
		/// </summary>
		public static T GetUserId<T>(this IIdentity identity)
		{
			identity.NotNull(nameof(identity));
			if (!(identity is ClaimsIdentity claimsIdentity))
			{
				return default(T);
			}

			var value = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (value == null)
			{
				return default(T);
			}

			return value.CastTo<T>();
		}

		/// <summary>
		/// 获取用户ID
		/// </summary>
		public static string GetUserId(this IIdentity identity)
		{
			identity.NotNull(nameof(identity));
			if (!(identity is ClaimsIdentity claimsIdentity))
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
			identity.NotNull(nameof(identity));
			if (!(identity is ClaimsIdentity claimsIdentity))
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
			identity.NotNull(nameof(identity));
			if (!(identity is ClaimsIdentity claimsIdentity))
			{
				return null;
			}

			return claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
		}

		/// <summary>
		/// 获取昵称
		/// </summary>
		public static string GetNickName(this IIdentity identity)
		{
			identity.NotNull(nameof(identity));
			if (!(identity is ClaimsIdentity claimsIdentity))
			{
				return null;
			}

			return claimsIdentity.FindFirst(ClaimTypes.GivenName)?.Value;
		}

		/// <summary>
		/// 移除指定类型的声明
		/// </summary>
		public static void RemoveClaim(this IIdentity identity, string claimType)
		{
			identity.NotNull(nameof(identity));
			if (!(identity is ClaimsIdentity claimsIdentity))
			{
				return;
			}

			var claim = claimsIdentity.FindFirst(claimType);
			if (claim == null)
			{
				return;
			}

			claimsIdentity.RemoveClaim(claim);
		}

		/// <summary>
		/// 获取所有角色
		/// </summary>
		public static string[] GetRoles(this IIdentity identity)
		{
			identity.NotNull(nameof(identity));
			if (!(identity is ClaimsIdentity claimsIdentity))
			{
				return new string[0];
			}

			return claimsIdentity.FindAll(ClaimTypes.Role).SelectMany(m =>
			{
				var roles = m.Value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
				return roles;
			}).ToArray();
		}
	}
}