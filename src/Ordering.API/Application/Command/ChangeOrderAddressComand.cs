using System;
using MSFramework.CQRS;


namespace Ordering.API.Application.Command
{
	public class ChangeOrderAddressCommand : ICommand
    {
		public Guid Id { get; set; }

		public int ExpectedVersion { get; set; }

		public string NewAddress { get; set; }
    }
}
