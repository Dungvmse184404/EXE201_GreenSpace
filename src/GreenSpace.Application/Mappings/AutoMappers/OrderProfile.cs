using AutoMapper;
using GreenSpace.Application.DTOs.Order;
using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Mappings.AutoMappers
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            // Order → OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => s.ShippingAddress != null ? "Pending" : ""))
                .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.Status == "Paid" ? "Success" : "Pending"));

            // OrderItem → OrderItemDto (THIS WAS MISSING!)
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.ProductId, o => o.MapFrom(s => s.Variant != null ? s.Variant.ProductId : Guid.Empty))
                .ForMember(d => d.ProductName, o => o.MapFrom(s =>
                    s.Variant != null && s.Variant.Product != null
                        ? s.Variant.Product.Name
                        : "Unknown Product"))
                .ForMember(d => d.VariantSku, o => o.MapFrom(s =>
                    s.Variant != null
                        ? s.Variant.Sku ?? ""
                        : ""));

            // CreateOrderDto → Order
            CreateMap<CreateOrderDto, Order>()
                .ForMember(d => d.OrderId, o => o.Ignore())
                .ForMember(d => d.UserId, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.TotalAmount, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.OrderItems, o => o.Ignore())
                .ForMember(d => d.Payments, o => o.Ignore())
                .ForMember(d => d.Promotions, o => o.Ignore());

            // CreateOrderItemDto → OrderItem
            CreateMap<CreateOrderItemDto, OrderItem>()
                .ForMember(d => d.ItemId, o => o.Ignore())
                .ForMember(d => d.OrderId, o => o.Ignore())
                .ForMember(d => d.PriceAtPurchase, o => o.Ignore())
                .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity));
        }
    }
}