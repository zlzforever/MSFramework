using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Common;

namespace MSFramework.Ef
{
	/// <summary>
	/// 设计时数据上下文实例工厂基类，用于执行数据迁移
	/// </summary>
	public abstract class DesignTimeDbContextFactoryBase<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
		where TDbContext : DbContext
	{
		/// <summary>
		/// 创建一个数据上下文实例
		/// </summary>
		/// <param name="args">参数</param>
		/// <returns></returns>
		public virtual TDbContext CreateDbContext(string[] args)
		{
			var services = new ServiceCollection();
			services.AddLogging();
			services.AddEntityFramework();
			Configure(services);
			var context = services.BuildServiceProvider().GetRequiredService<TDbContext>();
			Console.WriteLine("Create context success");
			return context;
		}

		protected abstract void Configure(IServiceCollection services);
	}
}