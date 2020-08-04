using System.ComponentModel.DataAnnotations;
using MSFramework.Application;

namespace Ordering.Application.Commands
{
	public class TestCommand1 : IRequest<string>
	{
		[Required]
		public string Name { get; set; }
	}
}