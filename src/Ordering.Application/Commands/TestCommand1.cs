using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Application.CQRS;

namespace Ordering.Application.Commands
{
	public class TestCommand1 : ICommand<string>
	{
		[Required, StringLength(10)] public string Name { get; set; }
	}
}