using System;
using MicroserviceFramework.Extensions;

namespace MicroserviceFramework.Domain;

public interface IHasLastModifierName
{
	/// <summary>
	/// 确保实现具有私有设置方法
	/// </summary>
	string LastModifierName { get; }
}