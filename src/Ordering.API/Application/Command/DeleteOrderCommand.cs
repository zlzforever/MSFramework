using System;
using MSFramework.Command;

namespace Ordering.API.Application.Command
{
	public class DeleteOrderCommand : ICommand
	{
		public Guid Id { get; set; }
		public int ExpectedVersion { get; set; }
	}
}
