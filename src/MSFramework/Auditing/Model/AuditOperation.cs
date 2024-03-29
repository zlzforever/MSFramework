using System;
using System.Collections.Generic;
using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace MicroserviceFramework.Auditing.Model;

public class AuditOperation : CreationAggregateRoot<ObjectId>
{
    /// <summary>
    /// 操作路径
    /// </summary>
    public string Url { get; private set; }

    /// <summary>
    /// 操作的 IP 地址
    /// </summary>
    public string IP { get; private set; }

    /// <summary>
    /// 设备 ID
    /// </summary>
    public string DeviceId { get; private set; }

    /// <summary>
    /// 设备型号
    /// </summary>
    public string DeviceModel { get; private set; }

    /// <summary>
    /// 经度
    /// </summary>
    public double? Lat { get; private set; }

    /// <summary>
    /// 纬度
    /// </summary>
    public double? Lng { get; private set; }

    /// <summary>
    /// 访问的 UserAgent
    /// </summary>
    public string UserAgent { get; private set; }

    /// <summary>
    /// 获取或设置审计数据信息集合
    /// </summary>
    public ICollection<AuditEntity> Entities { get; private set; }

    /// <summary>
    /// 请求结束时间
    /// </summary>
    public DateTimeOffset EndTime { get; private set; }

    /// <summary>
    /// 请求耗时
    /// </summary>
    public int Elapsed { get; private set; }

    public string TraceId { get; private set; }

    private AuditOperation() : base(ObjectId.GenerateNewId())
    {
        Entities = new List<AuditEntity>();
    }

    public AuditOperation(string url, string userAgent, string ip, string deviceModel, string deviceId, double? lat,
        double? lng, string traceId) : this()
    {
        IP = ip;
        Url = url;
        UserAgent = userAgent;
        DeviceModel = deviceModel;
        DeviceId = deviceId;
        Lat = lat;
        Lng = lng;
        TraceId = traceId;
    }

    public void AddEntities(IEnumerable<AuditEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.Operation = this;
            Entities.Add(entity);
        }
    }

    public void End()
    {
        EndTime = DateTimeOffset.Now;
        if (!CreationTime.HasValue)
        {
            Elapsed = 0;
        }
        else
        {
            Elapsed = (int)(EndTime - CreationTime.Value).TotalMilliseconds;
        }
    }

    public override string ToString()
    {
        return
            $"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'Ip': {IP}, 'UserAgent': {UserAgent}, 'EndedTime': {EndTime:yyyy-MM-dd HH:mm:ss}, 'Elapsed': {Elapsed} }}";
    }
}
