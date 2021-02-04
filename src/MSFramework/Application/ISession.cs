using System.Collections.Generic;

namespace MicroserviceFramework.Application
{
	/// <summary>
	/// todo: 优化实现，不要每次都从 Claims 里查找
	/// </summary>
	public interface ISession
	{
		string TraceIdentifier { get; }
		string UserId { get; }
		string UserName { get; }
		string Email { get; }
		string PhoneNumber { get; }
		HashSet<string> Roles { get; }
	}
}