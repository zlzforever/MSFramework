using AutoMapper;
using Ordering.Domain.AggregateRoots;

namespace Ordering.API.Controllers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Product, ProductDTO>();
    }
}
