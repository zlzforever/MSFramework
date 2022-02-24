using System.Collections.Generic;
using System.Linq;

namespace MicroserviceFramework.Application;

public static class SessionExtensions
{
	/// <summary>
	/// todo: 需要优化，不要每次都计算
	/// </summary>
	/// <param name="session"></param>
	/// <returns></returns>
	public static List<string> GetSubjects(this ISession session)
	{
		var roles = session.Roles.ToList();
		roles.Add(session.UserId);
		return roles;
	}
}