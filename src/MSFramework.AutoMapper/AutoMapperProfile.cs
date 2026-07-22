using AutoMapper;
using MicroserviceFramework.Common;

namespace MicroserviceFramework.AutoMapper;

/// <summary>
///     AutoMapper 映射配置，预配置 PaginationResult 的泛型映射
/// </summary>
public class AutoMapperProfile : Profile
{
    /// <summary>
    ///     注册 PaginationResult 泛型类型的映射
    /// </summary>
    public AutoMapperProfile()
    {
        CreateMap(typeof(PaginationResult<>), typeof(PaginationResult<>));
    }
}
