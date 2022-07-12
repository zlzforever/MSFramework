namespace MicroserviceFramework.Domain;

public interface IHasCreatorName
{
	/// <summary>
	/// 确保实现具有私有设置方法
	/// </summary>
	string CreatorName { get; }
}