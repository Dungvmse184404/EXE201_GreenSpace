using AutoMapper;
using GreenSpace.Application.DTOs.Promotion;
using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Mappings.AutoMappers
{
    public class PromotionProfile : Profile
    {
        public PromotionProfile()
        {
            CreateMap<Promotion, PromotionDto>().ReverseMap();
            CreateMap<CreatePromotionDto, Promotion>();
        }
    }
}
