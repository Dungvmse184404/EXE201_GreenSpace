using AutoMapper;
using GreenSpace.Application.DTOs.Cart;
using GreenSpace.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Mappings.AutoMappers
{
    public class CartProfile : Profile
    {
        public CartProfile()
        {
            CreateMap<Cart, CartDto>()
                .ForMember(d => d.TotalAmount, o => o.MapFrom(s =>
                    s.CartItems.Sum(ci => ci.Quantity!.Value * ci.Variant.Price)));

            CreateMap<CartItem, CartItemDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Variant.Product.Name))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.Variant.Price));
        }
    }
}
