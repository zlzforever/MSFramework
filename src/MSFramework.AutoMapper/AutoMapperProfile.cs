using AutoMapper;
using MicroserviceFramework.Common;

namespace MicroserviceFramework.AutoMapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap(typeof(PagedResult<>), typeof(PagedResult<>));
    }
}
