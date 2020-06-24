using System;

namespace MSFramework.Domain.Event
{
    public interface IEvent
    {
        /// <summary>
        /// 事件标识
        /// </summary>
        Guid EventId { get; set; }

        /// <summary>
        /// 事件发生时间
        /// </summary>
        DateTimeOffset EventTime { get; set; }

        /// <summary>
        /// 触发事件的对象
        /// </summary>
        object EventSource { get; set; }
    }
}