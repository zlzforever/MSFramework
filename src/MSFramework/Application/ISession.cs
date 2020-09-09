namespace MicroserviceFramework.Application
{
	public interface ISession
	{
		string UserId { get; }

		string UserName { get; }

		string Email { get; }

		string PhoneNumber { get; }

		string[] Roles { get; }
	}
}