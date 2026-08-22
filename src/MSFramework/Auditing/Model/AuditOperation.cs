using System;
using System.Collections.Generic;
using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace MicroserviceFramework.Auditing.Model;

/// <summary>
/// 当前审计 scope 的操作信息。实体按 <see cref="AddEntities"/> 的调用顺序和输入顺序保存；
/// 本类型不承诺并发安全。
/// </summary>
public class AuditOperation : CreationAggregateRoot<string>, IAuditObject
{
    /// <summary>
    /// 操作路径
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// 操作方法
    /// </summary>
    public string Method { get; private set; }

    /// <summary>请求 QueryString</summary>
    public string QueryString { get; init; }

    /// <summary>设备 IMEI</summary>
    public string IMEI { get; init; }

    /// <summary>客户端平台（Android/iOS）</summary>
    public string Platform { get; init; }

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

    /// <summary>海拔</summary>
    public float? Altitude { get; init; }

    /// <summary>
    /// 屏幕分辨率
    /// </summary>
    public string Screen { get; init; }

    /// <summary>
    /// 真机持续户外作业电量缓慢下降；模拟器电量常年 100% 不动
    /// </summary>
    public int? Battery { get; init; }

    /// <summary>
    /// 信号强度: 户外移动信号波动，模拟器信号固定值
    /// </summary>
    public int? Signal { get; init; }

    /// <summary>
    /// 模拟器版本多为通用测试版，线下真机版本分散
    /// </summary>
    public string OSVersion { get; init; }

    /// <summary>
    /// 模拟器精度通常固定几十米，真机户外会动态变化；室内精度差、户外精度高
    /// </summary>
    public float? Accuracy { get; init; }

    /// <summary>
    /// 静止设备航向固定，真人行走持续变化；模拟器航向不动
    /// </summary>
    public float? Bearing { get; init; }

    /// <summary>
    /// 指南针 Azimuth
    /// </summary>
    public float? Orientation { get; init; }

    /// <summary>
    /// gps/network/fused，模拟器大多只有 network，无真实 GPS 源
    /// </summary>
    public string LocationSource { get; init; }

    /// <summary>
    /// APP 底层检测模拟器环境直接标记，最直观
    /// </summary>
    public bool? Emulator { get; init; }

    /// <summary>
    /// 纬度（Latitude）
    /// </summary>
    public decimal? Lat { get; private set; }

    /// <summary>
    /// 经度（Longitude）
    /// </summary>
    public decimal? Lng { get; private set; }

    /// <summary>
    /// 访问的 UserAgent
    /// </summary>
    public string UserAgent { get; private set; }

    /// <summary>
    /// 获取或设置审计数据信息集合
    /// </summary>
    public List<AuditEntity> Entities { get; private set; }

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
        Entities = [];
    }

    /// <summary>
    /// 创建操作审计记录
    /// </summary>
    /// <param name="url">请求路径</param>
    /// <param name="userAgent">用户代理</param>
    /// <param name="ip">客户端 IP 地址</param>
    /// <param name="deviceModel">设备型号</param>
    /// <param name="deviceId">设备 ID</param>
    /// <param name="latitude">纬度</param>
    /// <param name="longitude">经度</param>
    /// <param name="traceId">跟踪标识</param>
    /// <param name="method">HTTP 方法</param>
    public AuditOperation(string url, string userAgent, string ip, string deviceModel, string deviceId, decimal? latitude,
        decimal? longitude, string traceId, string method) : this(ObjectId.GenerateNewId().ToString())
    {
        IP = ip;
        Path = url;
        UserAgent = userAgent;
        DeviceModel = deviceModel;
        DeviceId = deviceId;
        Lat = latitude;
        Lng = longitude;
        TraceId = traceId;
        Method = method;
    }

    /// <summary>
    /// 在当前审计 scope 内按输入顺序追加非空审计实体，并关联到当前操作。
    /// 本方法不承诺并发安全。
    /// </summary>
    /// <param name="entities">待添加的审计实体集合</param>
    public void AddEntities(IEnumerable<AuditEntity> entities)
    {
        if (entities == null)
        {
            return;
        }

        foreach (var entity in entities)
        {
            if (entity == null)
            {
                continue;
            }

            entity.SetOperation(this);
            Entities.Add(entity);
        }
    }

    /// <summary>
    /// 结束操作审计，记录结束时间和耗时
    /// </summary>
    public void End()
    {
        EndTime = DateTimeOffset.UtcNow;
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
    /// 返回操作审计的字符串表示
    /// </summary>
    /// <returns>操作审计信息字符串</returns>
    public override string ToString()
    {
        return
            $"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'Ip': {IP}, 'UserAgent': {UserAgent}, 'EndedTime': {EndTime:yyyy-MM-dd HH:mm:ss}, 'Elapsed': {Elapsed} }}";
    }

    // /// <summary>
    // ///
    // /// </summary>
    // /// <returns></returns>
    // /// <exception cref="NotImplementedException"></exception>
    // public AuditOperation Clone()
    // {
    //     return new AuditOperation(Path, UserAgent, IP, DeviceModel, DeviceId, Lat, Lng, TraceId);
    // }
}
