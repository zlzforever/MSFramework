using AutoMapper;
using Template.Application.DTO;
using Template.Domain;
using Template.Domain.AggregateRoot;

namespace Template.API.ViewObject
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<CreateClass1ViewObject, CreateClass1In>();
		}
	}
}