using AutoMapper;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.AutoMapper
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap(typeof(PagedResult<>), typeof(PagedResult<>));
		}
	}
}