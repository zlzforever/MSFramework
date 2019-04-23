using System;
using MSFramework.CQRS;


namespace Ordering.API.Application.Command
{
	public class DeleteOrderCommand : ICommand
	{
		public Guid Id { get; set; }
		public int ExpectedVersion { get; set; }
	}
}
