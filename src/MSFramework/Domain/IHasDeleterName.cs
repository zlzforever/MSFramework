namespace MicroserviceFramework.Domain;

public interface IHasDeleterName
{
    /// <summary>
    /// 确保实现具有私有设置方法
    /// </summary>
    string DeleterName { get; }
}
