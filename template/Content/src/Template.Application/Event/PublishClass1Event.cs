using System;

namespace Template.Application.Event
{
	public class PublishClass1Event : EventBus.Event
	{
		public Guid Class1Id { get; private set; }

		public PublishClass1Event(Guid class1Id)
		{
			Class1Id = class1Id;
		}
	}
}