using System;

namespace MicroserviceFramework.Ef
{
	public class EntityFrameworkOptions
	{
		/// <summary>
		/// 初始化一个<see cref="EntityFrameworkOptions"/>类型的新实例
		/// </summary>
		public EntityFrameworkOptions()
		{
			AutoMigrationEnabled = false;
		}

		/// <summary>
		/// 获取 上下文类型
		/// </summary>
		public Type DbContextType => string.IsNullOrEmpty(DbContextTypeName) ? null : Type.GetType(DbContextTypeName);

		/// <summary>
		/// 获取或设置 上下文类型全名
		/// </summary>
		public string DbContextTypeName { get; set; }

		/// <summary>
		/// 获取或设置 连接字符串
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// 启用事务
		/// </summary>
		public bool AutoTransactionsEnabled { get; set; } = true;

		/// <summary>
		/// 获取或设置 是否自动迁移
		/// </summary>
		public bool AutoMigrationEnabled { get; set; }

		public bool EnableSensitiveDataLogging { get; set; }

		/// <summary>
		/// 使用 unix 风格的表名、列名
		/// </summary>
		public bool UseUnderScoreCase { get; set; } = true;
	}
}