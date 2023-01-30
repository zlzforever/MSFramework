using AutoMapper;
using Template.Domain.Aggregates.Project;

namespace Template.Application.Project
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