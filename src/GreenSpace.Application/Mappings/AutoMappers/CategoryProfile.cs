using AutoMapper;
using GreenSpace.Application.DTOs.Category;
using GreenSpace.Domain.Models;


namespace GreenSpace.Application.Mappings.AutoMappers
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryDto>()
                .ForMember(d => d.ParentName, o => o.MapFrom(s => s.Parent != null ? s.Parent.Name : null))
                .ReverseMap();

            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();
        }
    }
}
