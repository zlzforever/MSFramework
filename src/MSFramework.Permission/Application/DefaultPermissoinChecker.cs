using System.Threading.Tasks;
using MSFramework.Permission.DomainService;
using MSFramework.Permission.Repository;

namespace MSFramework.Permission.Application
{
	public class DefaultPermissionChecker : IPermissionChecker
	{
		private readonly IPermissionRepository _repository;
		private readonly IPermissionHashService _permissionHashService;

		public DefaultPermissionChecker(IPermissionRepository repository, IPermissionHashService permissionHashService)
		{
			_repository = repository;
			_permissionHashService = permissionHashService;
		}

		public async Task<bool> HasPermissionAsync(string name, string type, string service, string path = "/")
		{
			var hash = _permissionHashService.Compute(name, type, service, path);
			return await _repository.ExistsPermissionAsync(hash);
		}

		public async Task<bool> HasPermissionAsync(string hash)
		{
			return await _repository.ExistsPermissionAsync(hash);
		}
	}
}