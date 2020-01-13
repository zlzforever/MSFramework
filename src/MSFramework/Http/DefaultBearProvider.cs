using System.Threading.Tasks;

namespace MSFramework.Http
{
	public class DefaultBearProvider : IBearProvider
	{
		public Task<string> GetTokenAsync()
		{
			return Task.FromResult<string>(null);
		}
	}
}