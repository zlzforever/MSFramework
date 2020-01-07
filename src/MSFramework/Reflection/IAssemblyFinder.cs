using System.Collections.Generic;
using System.Reflection;

namespace MSFramework.Reflection
{
	/// <summary>
	/// 定义程序集查找器
	/// </summary>
	public interface IAssemblyFinder
	{
		/// <summary>
		/// Gets all assemblies.
		/// </summary>
		/// <returns>List of assemblies</returns>
		List<Assembly> GetAllAssemblyList();
	}
}