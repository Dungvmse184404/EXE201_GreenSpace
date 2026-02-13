using AutoMapper;
using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Application.DTOs.User;
using GreenSpace.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Mappings.AutoMappers
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            // 1. User -> AuthResultDto (CHỈ 1 CHIỀU)
            CreateMap<User, AuthResultDto>()
                // Các trường token sẽ được gán sau ở Service, ignore để tránh lỗi hoặc warning
                .ForMember(d => d.AccessToken, o => o.Ignore())
                .ForMember(d => d.RefreshToken, o => o.Ignore())
                .ForMember(d => d.ExpiresAt, o => o.Ignore());

            // 2. User <-> UserDto (2 CHIEU)
            CreateMap<User, UserDto>()
                .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.Phone))
                .ForMember(d => d.Address, o => o.MapFrom(s => s.UserAddresses != null && s.UserAddresses.Any()
                    ? s.UserAddresses.FirstOrDefault(a => a.IsDefault)!.FullAddress
                    : null))
                .ForMember(d => d.Birthday, o => o.MapFrom(s => s.DateOfBirth.HasValue ? DateOnly.FromDateTime(s.DateOfBirth.Value) : (DateOnly?)null))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.IsActive == true ? "Active" : "Inactive"))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreateAt))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdateAt))
                .ReverseMap()
                .ForMember(d => d.DateOfBirth, o => o.MapFrom(s => s.Birthday.HasValue ? s.Birthday.Value.ToDateTime(new TimeOnly(0, 0)) : (DateTime?)null))
                .ForMember(d => d.Phone, o => o.MapFrom(s => s.PhoneNumber))
                // Ignore cac truong nhay cam khi map nguoc ve Entity
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.Carts, o => o.Ignore())
                .ForMember(d => d.Orders, o => o.Ignore())
                .ForMember(d => d.Ratings, o => o.Ignore())
                .ForMember(d => d.UserAddresses, o => o.Ignore());


            // 3. RegisterDto -> User (CHỈ 1 CHIỀU: Tạo mới)
            CreateMap<RegisterDto, User>()
                .ForMember(d => d.Username, o => o.MapFrom(s => s.Email)) // Thường lấy email làm username luôn
                .ForMember(d => d.CreateAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.UpdateAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
                .ForMember(d => d.Phone, o => o.MapFrom(s => s.PhoneNumber))
                // PasswordHash và Role xử lý ở Service, không map ở đây để an toàn
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.Role, o => o.Ignore());

            CreateMap<RegisterDto, User>()
            .ForMember(d => d.Username, o => o.MapFrom(s => s.Email)) // Lấy email làm username mặc định
            .ForMember(d => d.Phone, o => o.MapFrom(s => s.PhoneNumber)) // Khớp tên field
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true)) // Mặc định Active khi đăng ký
            .ForMember(d => d.CreateAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.UpdateAt, o => o.MapFrom(_ => DateTime.UtcNow))

            // QUAN TRỌNG: Ignore các trường nhạy cảm/logic để xử lý tay ở Service
            .ForMember(d => d.UserId, o => o.Ignore())      // Tự sinh
            .ForMember(d => d.PasswordHash, o => o.Ignore()) // Xử lý hash riêng
            .ForMember(d => d.Role, o => o.Ignore());        // Gán cứng "Customer" riêng


            CreateMap<User, UserDto>()
                 .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.Phone));


            CreateMap<InternalUserDto, User>()
                .ForMember(d => d.Username, o => o.MapFrom(s => s.Email))
                .ForMember(d => d.Phone, o => o.MapFrom(s => s.PhoneNumber))
                .ForMember(d => d.DateOfBirth, o => o.MapFrom(s => s.Birthday.HasValue ? s.Birthday.Value.ToDateTime(new TimeOnly(0, 0)) : (DateTime?)null))
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
                .ForMember(d => d.CreateAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.UpdateAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.UserId, o => o.Ignore())
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.Role, o => o.Ignore());

        }
    }
}
