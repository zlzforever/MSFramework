using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Repositories;

/// <summary>
/// EF 仓储基础接口，提供 DbContext 访问能力
/// </summary>
public interface IEfRepository
{
    /// <summary>
    /// 获取关联的 DbContext 实例
    /// </summary>
    DbContext DbContext { get; }
}
