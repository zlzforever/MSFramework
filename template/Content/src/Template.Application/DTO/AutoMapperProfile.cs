using AutoMapper;
using Template.Domain.AggregateRoot;

namespace Template.Application.DTO
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<Product, ProductOut>();
			CreateMap<Product, CreatProductOut>();
			CreateMap<CreateProductIn, Product>();
		}
	}
}