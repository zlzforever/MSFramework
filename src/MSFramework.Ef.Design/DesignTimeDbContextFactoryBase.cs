using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef.Design
{
	/// <summary>
	/// 设计时数据上下文实例工厂基类，用于执行数据迁移
	/// </summary>
	public abstract class DesignTimeDbContextFactoryBase<TDbContext> :
		IDesignTimeServices,
		IDesignTimeDbContextFactory<TDbContext>
		where TDbContext : DbContext
	{
		/// <summary>
		/// 创建一个数据上下文实例
		/// </summary>
		/// <param name="args">参数</param>
		/// <returns></returns>
		public virtual TDbContext CreateDbContext(string[] args)
		{
			var services = GetServiceProvider();
			return (TDbContext) services.CreateScope()
				.ServiceProvider.GetRequiredService(typeof(TDbContext));
		}

		protected abstract IServiceProvider GetServiceProvider();

		public abstract void ConfigureDesignTimeServices(IServiceCollection serviceCollection);
	}
}