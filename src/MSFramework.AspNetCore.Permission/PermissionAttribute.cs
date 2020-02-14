using System;

namespace MSFramework.AspNetCore.Permission
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class PermissionAttribute : Attribute
	{
		/// <summary>
		/// 权限名称
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 所属模块
		/// </summary>
		public string Module { get; set; }

		/// <summary>
		/// 权限描述
		/// </summary>
		public string Description { get; set; }
	}
}