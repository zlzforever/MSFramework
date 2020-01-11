using MSFramework.AspNetCore;

namespace Ordering.API
{
	public class TestService
	{
		[UnitOfWork]
		public string Get(string name)
		{
			return name;
		}
	}
}