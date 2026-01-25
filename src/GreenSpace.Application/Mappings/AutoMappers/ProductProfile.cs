using AutoMapper;
using GreenSpace.Application.DTOs.Product;
using GreenSpace.Application.DTOs.ProductVariant;
using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Mappings.AutoMappers
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : ""));

            CreateMap<CreateProductDto, Product>()
                .ForMember(d => d.ProductId, o => o.Ignore())
                .ForMember(d => d.IsActive, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore());

            CreateMap<UpdateProductDto, Product>()
                .ForMember(d => d.ProductId, o => o.Ignore())
                .ForMember(d => d.IsActive, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore());

            CreateMap<ProductVariant, ProductVariantDto>().ReverseMap();
        }
    }
}
