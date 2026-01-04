namespace MicroserviceFramework;

/// <summary>
///
/// </summary>
public abstract class InitializerBase : IInitializer
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public abstract void Start();

    /// <summary>
    ///
    /// </summary>
    public int Order { get; protected set; }
}
