using AutoMapper;
using MSFramework.Common;

namespace MSFramework.AutoMapper
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap(typeof(PagedResult<>), typeof(PagedResult<>));
		}
	}
}