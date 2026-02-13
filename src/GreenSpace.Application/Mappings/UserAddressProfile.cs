using AutoMapper;
using GreenSpace.Application.DTOs.UserAddress;
using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Mappings.AutoMappers;

public class UserAddressProfile : Profile
{
    public UserAddressProfile()
    {
        // Entity -> Response DTO
        CreateMap<UserAddress, UserAddressResponseDto>()
            .ForMember(d => d.FullAddress, o => o.MapFrom(s => s.FullAddress));

        // Create DTO -> Entity (manual mapping in service, but keep for reference)
        CreateMap<CreateUserAddressDto, UserAddress>()
            .ForMember(d => d.AddressId, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());

        // Update DTO -> Entity
        CreateMap<UpdateUserAddressDto, UserAddress>()
            .ForMember(d => d.AddressId, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());
    }
}


