namespace MSFramework.Application
{
	public interface ISession
	{
		string UserId { get; }

		string UserName { get; }
	}
}