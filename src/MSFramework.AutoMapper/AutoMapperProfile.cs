using AutoMapper;
using MicroserviceFramework.Common;

namespace MicroserviceFramework.AutoMapper;

/// <summary>
///
/// </summary>
public class AutoMapperProfile : Profile
{
    /// <summary>
    ///
    /// </summary>
    public AutoMapperProfile()
    {
        CreateMap(typeof(PaginationResult<>), typeof(PaginationResult<>));
    }
}
