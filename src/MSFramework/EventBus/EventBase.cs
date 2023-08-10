using System;
using MongoDB.Bson;

namespace MicroserviceFramework.EventBus;

/// <summary>
/// 事件基类
/// </summary>
public abstract class EventBase
{
    public static readonly Type Type = typeof(EventBase);

    /// <summary>
    /// 事件标识
    /// </summary>
    public string EventId { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>
    /// 事件时间
    /// </summary>
    public long EventTime { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

    public override string ToString() =>
        $"[{GetType().Name}] EventId = {EventId}, EventTime = {DateTimeOffset.FromUnixTimeMilliseconds(EventTime):YYYY-MM-DD HH:mm:ss}";
}
