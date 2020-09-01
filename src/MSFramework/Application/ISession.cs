namespace MicroserviceFramework.Application
{
	public interface ISession
	{
		string UserId { get; }

		string UserName { get; }
		
		string[] Roles { get; }
	}
}