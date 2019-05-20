using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	public class MSFrameworkSession : MSFrameworkSessionBase
	{
		private readonly IHttpContextAccessor _accessor;

		public MSFrameworkSession(IHttpContextAccessor accessor) 
		{
			_accessor = accessor;
		}

		public override string UserId => HttpContext?.User?.FindFirst("sub")?.Value;

		public override string UserName => HttpContext?.User?.FindFirst("name")?.Value;

		public HttpContext HttpContext => _accessor.HttpContext;

		public override Task<string> GetTokenAsync(string tokenName = "access_token")
		{
			return HttpContext.GetTokenAsync(tokenName);
		}
	}
}