using AutoMapper;
using GreenSpace.Application.DTOs.Order;
using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Mappings.AutoMappers
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(d => d.OrderId, o => o.MapFrom(s => (int)s.OrderId.GetHashCode()))
                .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => ""))
                .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => ""));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.ProductId, o => o.MapFrom(s => (int)s.Variant!.ProductId.GetHashCode()))
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Variant!.Product.Name))
                .ForMember(d => d.VariantSku, o => o.MapFrom(s => s.Variant!.Sku ?? ""));

            CreateMap<CreateOrderDto, Order>();
        }
    }
}
