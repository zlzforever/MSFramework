using System;
using MongoDB.Bson;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 事件基类
/// </summary>
public abstract record EventBase
{
    public static readonly Type EventBaseType = typeof(EventBase);

    /// <summary>
    /// 事件标识
    /// </summary>
    public string EventId { get; } = ObjectId.GenerateNewId().ToString();

    /// <summary>
    /// 事件时间
    /// </summary>
    public long EventTime { get; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

    public override string ToString() =>
        $"[{GetType().Name}] EventId = {EventId}, EventTime = {DateTimeOffset.FromUnixTimeMilliseconds(EventTime):YYYY-MM-DD HH:mm:ss}";
}
