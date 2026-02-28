using AutoMapper;
using GreenSpace.Application.DTOs.User;
using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Mappings.AutoMappers
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.DateOfBirth.HasValue
                    ? DateOnly.FromDateTime(src.DateOfBirth.Value)
                    : (DateOnly?)null))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreateAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdateAt));
        }
    }
}
