using System.Threading.Tasks;

namespace MSFramework.Permission.Repository
{
	public interface IPermissionRepository
	{
		/// <summary>
		/// 判断权限是否存在
		/// </summary>
		/// <param name="hash"></param>
		/// <returns></returns>
		Task<bool> ExistsPermissionAsync(string hash);
	}
}