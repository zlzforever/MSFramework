using AutoMapper;
using Template.Domain.Aggregates.Project;

namespace Template.Application.Project.V10
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Product, Dto.V10.ProductOut>();
            CreateMap<Product, Dto.V10.CreateProductOut>();
        }
    }
}
