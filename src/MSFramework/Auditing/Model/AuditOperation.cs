using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace MicroserviceFramework.Auditing.Model;

/// <summary>
/// 操作审计信息
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
        Entities = new ThreadSafeCollection<AuditEntity>();
        _collectedEntityKeys = [];
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
    /// 已收集审计实体的去重键集合（类型 + 实体标识 + 操作类型 + 属性变更快照）。
    /// <see cref="AuditEntity"/> 未重写相等性（引用相等），而每次 <c>GetAuditEntities()</c>
    /// 都会新建实例，残留处理器重复触发收集时无法用引用去重，
    /// 故以值语义三元组作为唯一键，保证同一实体的同一变更状态只收集一次。
    /// </summary>
    private readonly HashSet<AuditEntityKey> _collectedEntityKeys;

    /// <summary>
    /// 审计实体及其属性快照的值键。属性快照不同表示实体在不同保存批次中的不同变更，
    /// 即使操作类型相同也必须分别保留。
    /// </summary>
    private sealed class AuditEntityKey : IEquatable<AuditEntityKey>
    {
        private AuditEntityKey(string type, string entityId, OperationType operationType,
            IReadOnlyList<AuditPropertyKey> properties)
        {
            Type = type;
            EntityId = entityId;
            OperationType = operationType;
            Properties = properties;
        }

        private string Type { get; }

        private string EntityId { get; }

        private OperationType OperationType { get; }

        private IReadOnlyList<AuditPropertyKey> Properties { get; }

        public static AuditEntityKey From(AuditEntity entity)
        {
            var properties = (entity.Properties ?? []).Select(property =>
                    new AuditPropertyKey(property.Name, property.Type, property.OriginalValue, property.NewValue))
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .ThenBy(property => property.Type, StringComparer.Ordinal)
                .ThenBy(property => property.OriginalValue, StringComparer.Ordinal)
                .ThenBy(property => property.NewValue, StringComparer.Ordinal)
                .ToArray();

            return new AuditEntityKey(entity.Type, entity.EntityId, entity.OperationType, properties);
        }

        public bool Equals(AuditEntityKey other)
        {
            if (other == null || !string.Equals(Type, other.Type, StringComparison.Ordinal) ||
                !string.Equals(EntityId, other.EntityId, StringComparison.Ordinal) ||
                OperationType != other.OperationType || Properties.Count != other.Properties.Count)
            {
                return false;
            }

            for (var index = 0; index < Properties.Count; index++)
            {
                if (Properties[index] != other.Properties[index])
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AuditEntityKey);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Type, StringComparer.Ordinal);
            hash.Add(EntityId, StringComparer.Ordinal);
            hash.Add(OperationType);
            foreach (var property in Properties)
            {
                hash.Add(property);
            }

            return hash.ToHashCode();
        }
    }

    private readonly record struct AuditPropertyKey(string Name, string Type, string OriginalValue, string NewValue);

    /// <summary>
    /// 同步去重键集合与实体集合的修改，避免多个 DbContext 并发收集时出现部分提交。
    /// Entities 本身使用快照枚举的并发集合，外部枚举也不会与写入冲突。
    /// </summary>
    private readonly object _entitiesSync = new();

    /// <summary>
    /// 保持 ICollection 公共契约，同时为 Count、写入和枚举提供线程安全。
    /// 枚举返回固定快照，避免持有内部锁跨越调用方代码。
    /// </summary>
    private sealed class ThreadSafeCollection<T> : ICollection<T>
    {
        private readonly List<T> _items = [];
        private readonly object _sync = new();

        public int Count
        {
            get
            {
                lock (_sync)
                {
                    return _items.Count;
                }
            }
        }

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            lock (_sync)
            {
                _items.Add(item);
            }
        }

        public void Clear()
        {
            lock (_sync)
            {
                _items.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (_sync)
            {
                return _items.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_sync)
            {
                _items.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (_sync)
            {
                return _items.Remove(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_sync)
            {
                return _items.ToArray().AsEnumerable().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// 添加审计实体集合到当前操作，按实体值身份（类型 + 实体标识 + 操作类型）去重，保证同一实体只收集一次
    /// </summary>
    /// <param name="entities">审计实体集合</param>
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

            var key = AuditEntityKey.From(entity);
            lock (_entitiesSync)
            {
                // 残留处理器可能对同一请求重复触发收集（每次收集都会新建 AuditEntity 实例），
                // 按完整值快照去重，保证同一变更状态只进入集合一次。
                if (!_collectedEntityKeys.Add(key))
                {
                    continue;
                }

                entity.SetOperation(this);
                Entities.Add(entity);
            }
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
