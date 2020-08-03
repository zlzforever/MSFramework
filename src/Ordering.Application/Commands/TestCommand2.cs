using System.ComponentModel.DataAnnotations;
using MSFramework.Application;

namespace Ordering.Application.Commands
{
	public class TestCommand2
		: IRequest
	{
		[Required]
		public string Name { get; set; }
	}
}