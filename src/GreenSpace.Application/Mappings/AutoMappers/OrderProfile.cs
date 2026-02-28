using AutoMapper;
using GreenSpace.Application.DTOs.Order;
using GreenSpace.Domain.Constants;
using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Mappings.AutoMappers
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            // Order → OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(d => d.Items, o => o.MapFrom(s => s.OrderItems))
                .ForMember(d => d.PaymentMethod, o => o.MapFrom(s =>
                    s.Payments.OrderByDescending(p => p.CreatedAt).Select(p => p.PaymentMethod).FirstOrDefault()))
                .ForMember(d => d.PaymentStatus, o => o.MapFrom(s =>
                    s.Status == OrderStatus.Confirmed || s.Status == OrderStatus.Shipping || s.Status == OrderStatus.Completed
                        ? PaymentStatus.Success
                        : PaymentStatus.Pending));

            // OrderItem → OrderItemDto (THIS WAS MISSING!)
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.ProductId, o => o.MapFrom(s => s.Variant != null ? s.Variant.ProductId : Guid.Empty))
                .ForMember(d => d.ProductName, o => o.MapFrom(s =>
                    s.Variant != null && s.Variant.Product != null
                        ? s.Variant.Product.Name
                        : "Unknown Product"))
                .ForMember(d => d.VariantId, o => o.MapFrom(s => s.VariantId))
                .ForMember(d => d.VariantSku, o => o.MapFrom(s => s.Variant != null ? s.Variant.Sku : null))
                .ForMember(d => d.Color, o => o.MapFrom(s => s.Variant != null ? s.Variant.Color : null))
                .ForMember(d => d.SizeOrModel, o => o.MapFrom(s => s.Variant != null ? s.Variant.SizeOrModel : null))
                .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.Variant != null ? s.Variant.ImageUrl : null));

            // CreateOrderDto → Order
            CreateMap<CreateOrderDto, Order>()
                .ForMember(d => d.OrderId, o => o.Ignore())
                .ForMember(d => d.UserId, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.TotalAmount, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.OrderItems, o => o.Ignore())
                .ForMember(d => d.Payments, o => o.Ignore());

            // CreateOrderItemDto → OrderItem
            CreateMap<CreateOrderItemDto, OrderItem>()
                .ForMember(d => d.ItemId, o => o.Ignore())
                .ForMember(d => d.OrderId, o => o.Ignore())
                .ForMember(d => d.PriceAtPurchase, o => o.Ignore())
                .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity));
        }
    }
}