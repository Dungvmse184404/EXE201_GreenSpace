using AutoMapper;
using GreenSpace.Application.DTOs.User;
using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Mappings.AutoMappers
{
    public class UserAddressProfile : Profile
    {
        public UserAddressProfile()
        {
            CreateMap<UserAddress, UserAddressDto>().ReverseMap();

            CreateMap<CreateUserAddressDto, UserAddress>()
                .ForMember(d => d.AddressId, o => o.Ignore())
                .ForMember(d => d.UserId, o => o.Ignore());

            CreateMap<UpdateUserAddressDto, UserAddress>()
                .ForMember(d => d.AddressId, o => o.Ignore())
                .ForMember(d => d.UserId, o => o.Ignore());
        }
    }
}


