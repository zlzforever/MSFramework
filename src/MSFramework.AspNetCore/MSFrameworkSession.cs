using System.Threading.Tasks;
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

		public string UserId => _accessor.HttpContext.User.FindFirst("sub")?.Value;

		public string UserName => _accessor.HttpContext.User.FindFirst("name")?.Value;

		public async Task CommitAsync()
		{
			await _unitOfWork.CommitAsync();
		}

		public void Commit()
		{
			_unitOfWork.Commit();
		}
	}
}