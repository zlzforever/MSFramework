using Microsoft.EntityFrameworkCore;

namespace MSFramework.EntityFrameworkCore
{
	/// <summary>
	/// 定义<see cref="DbContextOptionsBuilder"/>创建器
	/// </summary>
	public interface IDbContextOptionsBuilderCreator
	{
		/// <summary>
		/// 获取 数据库类型名称，如SqlServer，MySql，Sqlite等
		/// </summary>
		string Type { get; }
        
		/// <summary>
		/// 创建<see cref="DbContextOptionsBuilder"/>对象
		/// </summary>
		/// <param name="connectionString">连接字符串</param>
		/// <returns></returns>
		DbContextOptionsBuilder Create(string connectionString);
	}
}