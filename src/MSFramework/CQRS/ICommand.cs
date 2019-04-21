using System;
using MSFramework.EventBus;

namespace MSFramework.CQRS
{
	public interface ICommand
	{
		Guid Id { get; set; }
		
		int ExpectedVersion { get; set; }
	}
}