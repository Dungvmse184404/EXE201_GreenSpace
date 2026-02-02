using AutoMapper;
using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.User;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services
{
    public class UserAddressService : IUserAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserAddressService> _logger;

        public UserAddressService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UserAddressService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IServiceResult<List<UserAddressDto>>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                var addresses = await _unitOfWork.UserAddressRepository.GetAllQueryable()
                    .Where(a => a.UserId == userId)
                    .ToListAsync();

                var dtos = _mapper.Map<List<UserAddressDto>>(addresses);
                return ServiceResult<List<UserAddressDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for user {UserId}", userId);
                return ServiceResult<List<UserAddressDto>>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<UserAddressDto>> GetByIdAsync(Guid addressId)
        {
            try
            {
                var address = await _unitOfWork.UserAddressRepository.GetByIdAsync(addressId);
                if (address == null)
                    return ServiceResult<UserAddressDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Address.NotFound);

                var dto = _mapper.Map<UserAddressDto>(address);
                return ServiceResult<UserAddressDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address {AddressId}", addressId);
                return ServiceResult<UserAddressDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<UserAddressDto>> CreateAsync(
            CreateUserAddressDto dto,
            Guid userId)
        {
            try
            {
                var address = _mapper.Map<UserAddress>(dto);
                address.UserId = userId;

                await _unitOfWork.UserAddressRepository.AddAsync(address);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<UserAddressDto>(address);
                return ServiceResult<UserAddressDto>.Success(result, ApiMessages.Address.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                return ServiceResult<UserAddressDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<UserAddressDto>> UpdateAsync(
     Guid addressId,
     UpdateUserAddressDto dto,
     Guid userId)
        {
            try
            {
                var address = await _unitOfWork.UserAddressRepository.GetByIdAsync(addressId);

                if (address == null)
                    return ServiceResult<UserAddressDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Address.NotFound);

                // Kiểm tra quyền sở hữu (Ownership)
                if (address.UserId != userId)
                    return ServiceResult<UserAddressDto>.Failure(ApiStatusCodes.Forbidden, "You can only update your own addresses");

                _mapper.Map(dto, address);

                await _unitOfWork.UserAddressRepository.UpdateAsync(address);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<UserAddressDto>(address);
                return ServiceResult<UserAddressDto>.Success(result, ApiMessages.Address.Updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId}", addressId);
                return ServiceResult<UserAddressDto>.Failure(ApiStatusCodes.InternalServerError, ex.Message);
            }
        }

        public async Task<IServiceResult<bool>> DeleteAsync(Guid addressId, Guid userId)
        {
            try
            {
                var address = await _unitOfWork.UserAddressRepository.GetByIdAsync(addressId);

                if (address == null)
                    return ServiceResult<bool>.Failure(ApiStatusCodes.NotFound, ApiMessages.Address.NotFound);

                //if (address.UserId != userId)
                //    return ServiceResult<bool>.Failure(ApiStatusCodes.Forbidden, ApiMessages.Address.NoPermit);

                await _unitOfWork.UserAddressRepository.RemoveAsync(address);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, ApiMessages.Address.Deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", addressId);
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, ex.Message);
            }
        }
    }
}