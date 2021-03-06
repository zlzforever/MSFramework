using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef
{
	public abstract class EntityTypeConfigurationBase<TEntity> : EntityTypeConfigurationBase<TEntity, DefaultDbContext>
		where TEntity : class
	{
	}

	/// <summary>
	/// 数据实体映射配置基类
	/// </summary>
	/// <typeparam name="TEntity">实体类型</typeparam>
	/// <typeparam name="TDbContext"></typeparam>
	public abstract class EntityTypeConfigurationBase<TEntity, TDbContext> :
		IEntityTypeConfiguration<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContextBase
	{
		/// <summary>
		/// 重写以实现实体类型各个属性的数据库配置
		/// </summary>
		/// <param name="builder">实体类型创建器</param>
		public virtual void Configure(EntityTypeBuilder<TEntity> builder)
		{
			// builder.HasKey("Id");
			//
			// var primaryKeyType = builder.Property("Id").Metadata.ClrType;
			// if (primaryKeyType == Consts.Types.String || primaryKeyType == Consts.Types.Guid)
			// {
			// 	builder.Property("Id").ValueGeneratedNever();
			// }


		}

		public override string ToString()
		{
			return $"DbContext: {typeof(TDbContext)}, EntityType: {typeof(TEntity)}";
		}
	}
}