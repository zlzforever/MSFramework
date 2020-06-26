using AutoMapper;
using Ordering.Domain.AggregateRoot;

namespace Ordering.API.Controllers
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<Product, ProductDTO>();
		}
	}
}