using AutoMapper;
using GreenSpace.Application.Common.Constants;
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
                    return ServiceResult<UserDto>.Failure(ApiStatusCodes.NotFound, "User not found or is inactive.");

                return ServiceResult<UserDto>.Success(_mapper.Map<UserDto>(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", userId);
                return ServiceResult<UserDto>.Failure(ApiStatusCodes.InternalServerError, "Error retrieving user profile.");
            }
        }

        public async Task<IServiceResult<List<UserDto>>> GetAllAsync()
        {
            try
            {
                var users = await _unitOfWork.UserRepository.GetAllQueryable()
                    .AsNoTracking()
                    .Where(u => u.IsActive == true)
                    .ToListAsync();

                return ServiceResult<List<UserDto>>.Success(_mapper.Map<List<UserDto>>(users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return ServiceResult<List<UserDto>>.Failure(ApiStatusCodes.InternalServerError, "Could not retrieve user list.");
            }
        }

        public async Task<IServiceResult<UserDto>> UpdateAsync(Guid userId, UpdateUserDto dto)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult<UserDto>.Failure(ApiStatusCodes.NotFound, "User not found.");

                // Only update fields that are explicitly provided (not null)

                if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
                {
                    if (await _unitOfWork.UserRepository.EmailExistsAsync(dto.Email))
                        return ServiceResult<UserDto>.Failure(ApiStatusCodes.Conflict, "Email is already in use by another account.");
                    user.Email = dto.Email;
                }

                if (!string.IsNullOrEmpty(dto.PhoneNumber) && dto.PhoneNumber != user.Phone)
                {
                    if (await _unitOfWork.UserRepository.PhoneExistsAsync(dto.PhoneNumber))
                        return ServiceResult<UserDto>.Failure(ApiStatusCodes.Conflict, "Phone number is already in use.");
                    user.Phone = dto.PhoneNumber;
                }

                if (!string.IsNullOrEmpty(dto.FullName))
                {
                    var names = dto.FullName.Trim().Split(' ', 2);
                    user.FirstName = names.Length > 0 ? names[0] : "";
                    user.LastName = names.Length > 1 ? names[1] : "";
                }

                user.UpdateAt = DateTime.UtcNow;

                await _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<UserDto>.Success(_mapper.Map<UserDto>(user), "User profile updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return ServiceResult<UserDto>.Failure(ApiStatusCodes.InternalServerError, "Failed to update user profile.");
            }
        }

        public async Task<IServiceResult<bool>> DeactivateAsync(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult<bool>.Failure(ApiStatusCodes.NotFound, "User not found.");

                user.IsActive = false;
                user.UpdateAt = DateTime.UtcNow;

                await _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, "User has been deactivated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, "Failed to deactivate user.");
            }
        }
    }
}