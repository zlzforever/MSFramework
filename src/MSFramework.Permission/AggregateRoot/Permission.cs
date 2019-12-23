using System.ComponentModel.DataAnnotations;
using MSFramework.Data;
using MSFramework.Domain;
using MSFramework.Permission.DomainService;

namespace MSFramework.Permission.AggregateRoot
{
	public class Permission : ModificationAuditedAggregateRoot
	{
		/// <summary>
		/// 权限名字
		/// </summary>
		[StringLength(255)]
		public string Name { get; private set; }

		/// <summary>
		/// 权限类型
		/// </summary>
		[StringLength(255)]
		public string Type { get; private set; }

		/// <summary>
		/// 权限所属服务
		/// </summary>
		[StringLength(255)]
		public string Service { get; private set; }

		/// <summary>
		/// 权限的 HASH, 不同的权限会有不同的 HASH 算法, 比如 API 则 HASH 为： {PATH}_{SERVICE} 的 MD5
		/// </summary>
		[StringLength(32)]
		public string Hash { get; private set; }

		/// <summary>
		/// API 权限的路径
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// 权限描述
		/// </summary>
		public string Description { get; private set; }

		private Permission()
		{
		}

		public Permission(IPermissionHashService permissionHashService, string name, string type, string service,
			string path = "/",
			string description = "")
		{
			permissionHashService.NotNull(nameof(permissionHashService));
			name.NotNull(nameof(name));
			type.NotNull(nameof(type));

			Name = name;
			Type = type;
			Service = service;
			Path = path;
			Description = description;
			Hash = permissionHashService.Compute(name,type,service,path);
		}
	}
}