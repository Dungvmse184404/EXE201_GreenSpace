using AutoMapper;
using GreenSpace.Application.DTOs.User;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IServiceResult<UserDto>> GetByIdAsync(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null || user.IsActive != true)
                    return ServiceResult<UserDto>.Failure("User not found");

                var dto = _mapper.Map<UserDto>(user);
                return ServiceResult<UserDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", userId);
                return ServiceResult<UserDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<List<UserDto>>> GetAllAsync()
        {
            try
            {
                var users = await _unitOfWork.UserRepository.GetAllQueryable()
                    .Where(u => u.IsActive == true)
                    .ToListAsync();

                var dtos = _mapper.Map<List<UserDto>>(users);
                return ServiceResult<List<UserDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return ServiceResult<List<UserDto>>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<UserDto>> UpdateAsync(Guid userId, UpdateUserDto dto)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult<UserDto>.Failure("User not found");

                if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
                {
                    var emailExists = await _unitOfWork.UserRepository.EmailExistsAsync(dto.Email);
                    if (emailExists)
                        return ServiceResult<UserDto>.Failure("Email already in use");
                    user.Email = dto.Email;
                }

                if (!string.IsNullOrEmpty(dto.PhoneNumber) && dto.PhoneNumber != user.Phone)
                {
                    var phoneExists = await _unitOfWork.UserRepository.PhoneExistsAsync(dto.PhoneNumber);
                    if (phoneExists)
                        return ServiceResult<UserDto>.Failure("Phone already in use");
                    user.Phone = dto.PhoneNumber;
                }

                if (!string.IsNullOrEmpty(dto.FullName))
                {
                    var names = dto.FullName.Split(' ', 2);
                    user.FirstName = names.Length > 0 ? names[0] : "";
                    user.LastName = names.Length > 1 ? names[1] : "";
                }

                user.IsActive = dto.IsActive;
                user.UpdateAt = DateTime.UtcNow;

                await _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<UserDto>(user);
                return ServiceResult<UserDto>.Success(result, "User updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return ServiceResult<UserDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<bool>> DeactivateAsync(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult<bool>.Failure("User not found");

                user.IsActive = false;
                user.UpdateAt = DateTime.UtcNow;

                await _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, "User deactivated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                return ServiceResult<bool>.Failure($"Error: {ex.Message}");
            }
        }
    }
}