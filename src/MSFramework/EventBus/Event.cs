using System;

namespace MSFramework.EventBus
{
    public abstract class Event : IEvent
    {
        public Event()
        {
            Id = Guid.NewGuid();
            CreationTime = DateTime.UtcNow;
        }

        public Guid Id { get; }

        public DateTime CreationTime { get; }
    }
}