namespace MicroserviceFramework.Application;

/// <summary>
/// 定义可通过 <see cref="ISession.GetValue"/> 从请求头中提取的设备字段，
/// 每个实例自带对应的 Header Key。
/// </summary>
public sealed class SessionField
{
    /// <summary>
    /// 对应的请求 Header Key
    /// </summary>
    public string HeaderKey { get; }

    private SessionField(string headerKey)
    {
        HeaderKey = headerKey;
    }

    /// <summary>指南针 Azimuth</summary>
    public static readonly SessionField Orientation = new("z-orientation");

    /// <summary>设备标识</summary>
    public static readonly SessionField DeviceId = new("z-device-id");

    /// <summary>设备型号</summary>
    public static readonly SessionField DeviceModel = new("z-device-model");

    /// <summary>设备 IMEI</summary>
    public static readonly SessionField IMEI = new("z-imei");

    /// <summary>平台（Android/iOS）</summary>
    public static readonly SessionField Platform = new("z-platform");

    /// <summary>纬度</summary>
    public static readonly SessionField Latitude = new("z-latitude");

    /// <summary>经度</summary>
    public static readonly SessionField Longitude = new("z-longitude");

    /// <summary>海拔</summary>
    public static readonly SessionField Altitude = new("z-altitude");

    /// <summary>屏幕分辨率</summary>
    public static readonly SessionField Screen = new("z-screen");

    /// <summary>电量百分比</summary>
    public static readonly SessionField Battery = new("z-battery");

    /// <summary>信号强度</summary>
    public static readonly SessionField Signal = new("z-signal");

    /// <summary>操作系统版本</summary>
    public static readonly SessionField OSVersion = new("z-os-version");

    /// <summary>定位精度</summary>
    public static readonly SessionField Accuracy = new("z-accuracy");

    /// <summary>航向</summary>
    public static readonly SessionField Bearing = new("z-bearing");

    /// <summary>定位来源（gps/network/fused）</summary>
    public static readonly SessionField LocationSource = new("z-location-source");

    /// <summary>是否模拟器</summary>
    public static readonly SessionField Emulator = new("z-emulator");
}
