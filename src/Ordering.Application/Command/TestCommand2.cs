using System.ComponentModel.DataAnnotations;
using MSFramework.Application;

namespace Ordering.Application.Command
{
	public class TestCommand2
		: ICommand
	{
		[Required]
		public string Name { get; set; }
	}
}