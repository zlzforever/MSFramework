using System;
using MSFramework.Command;

namespace Ordering.API.Application.Command
{
	public class DeleteOrderCommand : ICommand
	{
		public Guid OrderId { get; set; }
		public long Version { get; set; }
	}
}
