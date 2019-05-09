using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	public class MSFrameworkSession : IMSFrameworkSession
	{
		private readonly IHttpContextAccessor _accessor;
		private readonly IUnitOfWork _unitOfWork;

		public MSFrameworkSession(IHttpContextAccessor accessor, IUnitOfWork unitOfWork)
		{
			_accessor = accessor;
			_unitOfWork = unitOfWork;
		}

		public string UserId => HttpContext?.User?.FindFirst("sub")?.Value;

		public string UserName => HttpContext?.User?.FindFirst("name")?.Value;

		public HttpContext HttpContext => _accessor.HttpContext;

		public async Task CommitAsync()
		{
			await _unitOfWork.CommitAsync();
		}

		public void Commit()
		{
			_unitOfWork.Commit();
		}

		public Task<string> GetTokenAsync(string tokenName = "access_token")
		{
			return HttpContext.GetTokenAsync(tokenName);
		}
	}
}