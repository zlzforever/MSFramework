using AutoMapper;
using MSFramework.Domain;
using Template.Application.DTO;
using Template.Domain.AggregateRoot;

namespace Template.API.ViewObject
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<CreateProductViewObject, CreateProductIn>().ForMember(x => x.Type, opt =>
				opt.MapFrom(x => Enumeration.FromValue<ProductType>(x.Type)));
		}
	}
}