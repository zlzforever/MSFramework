using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Application.CQRS;

namespace Ordering.Application.Commands
{
	public class TestCommand2
		: ICommand
	{
		[Required]
		public string Name { get; set; }
	}
}