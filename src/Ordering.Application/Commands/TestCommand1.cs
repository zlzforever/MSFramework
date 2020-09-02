using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Application.CQRS.Command;

namespace Ordering.Application.Commands
{
	public class TestCommand1 : ICommand<string>
	{
		[Required]
		public string Name { get; set; }
	}
}