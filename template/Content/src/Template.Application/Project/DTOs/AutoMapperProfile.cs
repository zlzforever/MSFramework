using AutoMapper;
using Template.Domain.Aggregates.Project;

namespace Template.Application.Project.DTOs
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<Product, ProductOut>();
			CreateMap<Product, CreatProductOut>();
		}
	}
}