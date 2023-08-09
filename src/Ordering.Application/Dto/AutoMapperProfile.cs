using AutoMapper;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.Dto;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Order, OrderDto>();
        CreateMap<OrderExtra, OrderDto.OrderExtraDto>();
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<OrderProduct, OrderItemDto.OrderProductDto>();
    }
}
