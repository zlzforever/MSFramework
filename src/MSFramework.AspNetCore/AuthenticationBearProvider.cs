using System.Threading.Tasks;
using MSFramework.Data;
using MSFramework.Domain;
using MSFramework.Http;

namespace MSFramework.AspNetCore
{
	public class AuthenticationBearProvider : IBearProvider
	{
		private readonly MSFrameworkSession _accessor;

		public AuthenticationBearProvider(IMSFrameworkSession accessor)
		{
			accessor.NotNull(nameof(accessor));
			_accessor = (MSFrameworkSession) accessor;
		}

		public async Task<string> GetTokenAsync()
		{
			return await _accessor.GetTokenAsync();
		}
	}
}