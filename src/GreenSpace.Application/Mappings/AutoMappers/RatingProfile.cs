using AutoMapper;
using GreenSpace.Application.DTOs.Rating;
using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Mappings.AutoMappers
{
    public class RatingProfile : Profile
    {
        public RatingProfile()
        {
            CreateMap<Rating, RatingDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : ""))
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ?
                    $"{s.User.FirstName} {s.User.LastName}".Trim() : ""));


            CreateMap<CreateRatingDto, Rating>()
                .ForMember(d => d.RatingId, o => o.Ignore())
                .ForMember(d => d.UserId, o => o.Ignore())
                .ForMember(d => d.CreateDate, o => o.MapFrom(s => DateTime.UtcNow))
                .ForMember(d => d.Product, o => o.Ignore())
                .ForMember(d => d.User, o => o.Ignore());

            CreateMap<UpdateRatingDto, Rating>()
                .ForMember(d => d.RatingId, o => o.Ignore())
                .ForMember(d => d.ProductId, o => o.Ignore())
                .ForMember(d => d.UserId, o => o.Ignore())
                .ForMember(d => d.CreateDate, o => o.Ignore());
        }
    }
}