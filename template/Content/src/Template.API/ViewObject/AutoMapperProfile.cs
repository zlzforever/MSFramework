using AutoMapper;
using Template.Application.DTO;

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