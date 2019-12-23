using System.Threading.Tasks;

namespace MSFramework.Permission.Application
{
	/// <summary>
	/// 
	/// </summary>
	public interface IPermissionChecker
	{
		Task<bool> HasPermissionAsync(string name, string type, string service, string path = "/");

		Task<bool> HasPermissionAsync(string hash);
	}
}