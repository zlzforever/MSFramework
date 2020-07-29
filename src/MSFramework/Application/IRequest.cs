namespace MSFramework.Application
{
	public interface IRequest
	{
	}

	public interface IRequest<TResponse> : IRequest
	{
	}
}