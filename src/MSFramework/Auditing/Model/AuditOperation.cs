using System;
using System.Collections.Generic;
using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace MicroserviceFramework.Auditing.Model;

/// <summary>
/// 操作审计信息
/// </summary>
public class AuditOperation : CreationAggregateRoot<string>
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

    /// <summary>
    /// 跟踪标识
    /// </summary>
    public string TraceId { get; private set; }

    private AuditOperation(string id) : base(id)
    {
        Entities = new List<AuditEntity>();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="url"></param>
    /// <param name="userAgent"></param>
    /// <param name="ip"></param>
    /// <param name="deviceModel"></param>
    /// <param name="deviceId"></param>
    /// <param name="lat"></param>
    /// <param name="lng"></param>
    /// <param name="traceId"></param>
    public AuditOperation(string url, string userAgent, string ip, string deviceModel, string deviceId, double? lat,
        double? lng, string traceId) : this(ObjectId.GenerateNewId().ToString())
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="entities"></param>
    public void AddEntities(IEnumerable<AuditEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.Operation = this;
            Entities.Add(entity);
        }
    }

    /// <summary>
    ///
    /// </summary>
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

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'Ip': {IP}, 'UserAgent': {UserAgent}, 'EndedTime': {EndTime:yyyy-MM-dd HH:mm:ss}, 'Elapsed': {Elapsed} }}";
    }
}
