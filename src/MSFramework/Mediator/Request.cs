namespace MicroserviceFramework.Mediator;

public abstract record Request;

public abstract record Request<TResponse>
{
    public override string ToString()
    {
        return $"Response: {typeof(TResponse).FullName}";
    }
}
