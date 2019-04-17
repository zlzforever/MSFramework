using System;

namespace MSFramework.EventBus
{
    public class Event
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