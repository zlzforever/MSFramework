using System;
using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;

namespace MicroserviceFramework.Mediator
{
	public interface IMessageHandler<in TMessage> : IScopeDependency, IDisposable
		where TMessage : IMessage
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task HandleAsync(TMessage message);
	}
}