using Microsoft.Extensions.Logging;
using MSFramework.Application;
using MSFramework.Domain;

namespace Ordering.API.Application.Services
{
	public interface ITestService : IApplicationService
	{
	}

	public class TestService : ApplicationServiceBase, ITestService
	{
		public TestService(IMSFrameworkSession session, ILogger<TestService> logger) : base(session, logger)
		{
		}
	}
}