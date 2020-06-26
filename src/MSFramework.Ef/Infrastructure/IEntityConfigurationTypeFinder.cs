using System;

namespace MSFramework.Ef.Infrastructure
{
	/// <summary>
	/// 定义实体类配置类型查找器
	/// </summary>
	public interface IEntityConfigurationTypeFinder
	{
		void Initialize();
		
		/// <summary>
		/// 获取指定上下文类型的实体配置注册信息
		/// </summary>
		/// <param name="dbContextType">数据上下文类型</param>
		/// <returns></returns>
		IEntityRegister[] GetEntityRegisters(Type dbContextType);

		/// <summary>
		/// 获取 实体类所属的数据上下文类
		/// </summary>
		/// <param name="entityType">实体类型</param>
		/// <returns>数据上下文类型</returns>
		Type GetDbContextTypeForEntity(Type entityType);
	}
}