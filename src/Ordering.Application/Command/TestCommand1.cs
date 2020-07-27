using System.ComponentModel.DataAnnotations;
using MSFramework.Application;

namespace Ordering.Application.Command
{
	public class TestCommand1 : ICommand<string>
	{
		[Required]
		public string Name { get; set; }
	}
}