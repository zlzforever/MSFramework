namespace MSFramework.Domain
{
	public interface ISession
	{
		string UserId { get; }

		string UserName { get; }
	}
}