using System.Threading.Tasks;

namespace MSFramework.Http
{
	public interface IBearProvider
	{
		Task<string> GetTokenAsync();
	}
}