using AutoMapper;
using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.UserAddress;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services;

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

    public async Task<IServiceResult> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var addresses = await _unitOfWork.UserAddressRepository.GetByUserIdAsync(userId);
            var dtos = _mapper.Map<List<UserAddressResponseDto>>(addresses);
            return ServiceResult<List<UserAddressResponseDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting addresses for user {UserId}", userId);
            return ServiceResult.Failure(ApiStatusCodes.InternalServerError, ex.Message);
        }
    }

    public async Task<IServiceResult> GetByIdAsync(Guid addressId, Guid userId)
    {
        try
        {
            var address = await _unitOfWork.UserAddressRepository.GetByIdAndUserIdAsync(addressId, userId);

            if (address == null)
                return ServiceResult.Failure(ApiStatusCodes.NotFound, ApiMessages.Address.NotFound);

            var dto = _mapper.Map<UserAddressResponseDto>(address);
            return ServiceResult<UserAddressResponseDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting address {AddressId}", addressId);
            return ServiceResult.Failure(ApiStatusCodes.InternalServerError, ex.Message);
        }
    }

    public async Task<IServiceResult> GetDefaultAsync(Guid userId)
    {
        try
        {
            var address = await _unitOfWork.UserAddressRepository.GetDefaultByUserIdAsync(userId);

            if (address == null)
                return ServiceResult.Failure(ApiStatusCodes.NotFound, "No default address found");

            var dto = _mapper.Map<UserAddressResponseDto>(address);
            return ServiceResult<UserAddressResponseDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default address for user {UserId}", userId);
            return ServiceResult.Failure(ApiStatusCodes.InternalServerError, ex.Message);
        }
    }

    public async Task<IServiceResult> CreateAsync(CreateUserAddressDto dto, Guid userId)
    {
        try
        {
            // Neu dat lam default, reset cac dia chi khac
            if (dto.IsDefault)
            {
                await _unitOfWork.UserAddressRepository.ResetDefaultAsync(userId);
            }

            var address = new UserAddress
            {
                AddressId = Guid.NewGuid(),
                UserId = userId,
                Province = dto.Province,
                District = dto.District,
                Ward = dto.Ward,
                StreetAddress = dto.StreetAddress,
                Label = dto.Label,
                IsDefault = dto.IsDefault,
                CreatedAt = DateTime.UtcNow
            };

            // Neu la dia chi dau tien, tu dong dat lam default
            var existingAddresses = await _unitOfWork.UserAddressRepository.GetByUserIdAsync(userId);
            if (!existingAddresses.Any())
            {
                address.IsDefault = true;
            }

            await _unitOfWork.UserAddressRepository.AddAsync(address);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<UserAddressResponseDto>(address);
            return ServiceResult<UserAddressResponseDto>.Success(result, ApiMessages.Address.Created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating address for user {UserId}", userId);
            return ServiceResult.Failure(ApiStatusCodes.InternalServerError, ex.Message);
        }
    }

    public async Task<IServiceResult> UpdateAsync(Guid addressId, UpdateUserAddressDto dto, Guid userId)
    {
        try
        {
            var address = await _unitOfWork.UserAddressRepository.GetByIdAndUserIdAsync(addressId, userId);

            if (address == null)
                return ServiceResult.Failure(ApiStatusCodes.NotFound, ApiMessages.Address.NotFound);

            // Neu dat lam default, reset cac dia chi khac
            if (dto.IsDefault && !address.IsDefault)
            {
                await _unitOfWork.UserAddressRepository.ResetDefaultAsync(userId);
            }

            address.Province = dto.Province;
            address.District = dto.District;
            address.Ward = dto.Ward;
            address.StreetAddress = dto.StreetAddress;
            address.Label = dto.Label;
            address.IsDefault = dto.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UserAddressRepository.UpdateAsync(address);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<UserAddressResponseDto>(address);
            return ServiceResult<UserAddressResponseDto>.Success(result, ApiMessages.Address.Updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address {AddressId}", addressId);
            return ServiceResult.Failure(ApiStatusCodes.InternalServerError, ex.Message);
        }
    }

    public async Task<IServiceResult> DeleteAsync(Guid addressId, Guid userId)
    {
        try
        {
            var address = await _unitOfWork.UserAddressRepository.GetByIdAndUserIdAsync(addressId, userId);

            if (address == null)
                return ServiceResult.Failure(ApiStatusCodes.NotFound, ApiMessages.Address.NotFound);

            var wasDefault = address.IsDefault;

            await _unitOfWork.UserAddressRepository.RemoveAsync(address);
            await _unitOfWork.SaveChangesAsync();

            // Neu xoa dia chi default, dat dia chi dau tien con lai lam default
            if (wasDefault)
            {
                var remainingAddresses = await _unitOfWork.UserAddressRepository.GetByUserIdAsync(userId);
                var firstAddress = remainingAddresses.FirstOrDefault();
                if (firstAddress != null)
                {
                    firstAddress.IsDefault = true;
                    await _unitOfWork.UserAddressRepository.UpdateAsync(firstAddress);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            return ServiceResult.Success(ApiMessages.Address.Deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address {AddressId}", addressId);
            return ServiceResult.Failure(ApiStatusCodes.InternalServerError, ex.Message);
        }
    }

    public async Task<IServiceResult> SetDefaultAsync(Guid addressId, Guid userId)
    {
        try
        {
            var address = await _unitOfWork.UserAddressRepository.GetByIdAndUserIdAsync(addressId, userId);

            if (address == null)
                return ServiceResult.Failure(ApiStatusCodes.NotFound, ApiMessages.Address.NotFound);

            if (address.IsDefault)
                return ServiceResult.Success("Address is already default");

            // Reset tat ca ve false
            await _unitOfWork.UserAddressRepository.ResetDefaultAsync(userId);

            // Set dia chi nay lam default
            address.IsDefault = true;
            address.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UserAddressRepository.UpdateAsync(address);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<UserAddressResponseDto>(address);
            return ServiceResult<UserAddressResponseDto>.Success(result, "Default address updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default address {AddressId}", addressId);
            return ServiceResult.Failure(ApiStatusCodes.InternalServerError, ex.Message);
        }
    }
}
