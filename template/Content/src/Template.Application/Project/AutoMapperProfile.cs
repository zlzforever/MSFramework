using AutoMapper;
using Template.Domain.Aggregates.Project;

namespace Template.Application.Project
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<Product, Dtos.V10.ProductOut>();
			CreateMap<Product, Dtos.V10.CreateProductOut>();
		}
	}
}