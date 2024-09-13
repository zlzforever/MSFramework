namespace MicroserviceFramework.Mediator;

/// <summary>
///
/// </summary>
public abstract record Request;

/// <summary>
///
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public abstract record Request<TResponse>
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Response: {typeof(TResponse).FullName}";
    }
}
