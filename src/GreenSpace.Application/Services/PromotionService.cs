using AutoMapper;
using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.Promotion;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PromotionService> _logger;

        public PromotionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PromotionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IServiceResult<List<PromotionDto>>> GetAllAsync()
        {
            try
            {
                var promotions = await _unitOfWork.PromotionRepository.GetAllQueryable()
                    .AsNoTracking()
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return ServiceResult<List<PromotionDto>>.Success(_mapper.Map<List<PromotionDto>>(promotions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all promotions");
                return ServiceResult<List<PromotionDto>>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<PromotionDto>> GetByIdAsync(Guid promotionId)
        {
            try
            {
                var promotion = await _unitOfWork.PromotionRepository.GetByIdAsync(promotionId);
                if (promotion == null)
                    return ServiceResult<PromotionDto>.Failure(ApiStatusCodes.NotFound, "Promotion not found.");

                return ServiceResult<PromotionDto>.Success(_mapper.Map<PromotionDto>(promotion));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting promotion {PromotionId}", promotionId);
                return ServiceResult<PromotionDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<List<PromotionDto>>> GetActivePromotionsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var promotions = await _unitOfWork.PromotionRepository.GetAllQueryable()
                    .Where(p => p.IsActive == true &&
                                (p.StartDate == null || p.StartDate <= now) &&
                                (p.EndDate == null || p.EndDate >= now) &&
                                (p.MaxUsage == null || p.UsedCount < p.MaxUsage))
                    .ToListAsync();

                var dtos = _mapper.Map<List<PromotionDto>>(promotions);
                return ServiceResult<List<PromotionDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active promotions");
                return ServiceResult<List<PromotionDto>>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<PromotionDto>> CreateAsync(CreatePromotionDto dto)
        {
            try
            {
                // Check if code already exists
                if (!string.IsNullOrEmpty(dto.Code))
                {
                    var existing = await _unitOfWork.PromotionRepository.GetAllQueryable()
                        .AnyAsync(p => p.Code != null && p.Code.ToLower() == dto.Code.ToLower());

                    if (existing)
                        return ServiceResult<PromotionDto>.Failure(ApiStatusCodes.BadRequest, "Mã voucher đã tồn tại");
                }

                var promotion = _mapper.Map<Promotion>(dto);
                promotion.IsActive = true;
                promotion.UsedCount = 0;
                promotion.CreatedAt = DateTime.UtcNow;
                promotion.UpdatedAt = DateTime.UtcNow;
                promotion.CreateAt = DateTime.UtcNow;
                promotion.UpdateAt = DateTime.UtcNow;

                await _unitOfWork.PromotionRepository.AddAsync(promotion);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<PromotionDto>(promotion);
                return ServiceResult<PromotionDto>.Success(result, "Promotion created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating promotion");
                return ServiceResult<PromotionDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<PromotionDto>> UpdateAsync(Guid promotionId, UpdatePromotionDto dto)
        {
            try
            {
                var promotion = await _unitOfWork.PromotionRepository.GetByIdAsync(promotionId);
                if (promotion == null)
                    return ServiceResult<PromotionDto>.Failure(ApiStatusCodes.NotFound, "Promotion not found.");

                // Check code uniqueness if changed
                if (!string.IsNullOrEmpty(dto.Code) && dto.Code.ToLower() != promotion.Code?.ToLower())
                {
                    var codeExists = await _unitOfWork.PromotionRepository.GetAllQueryable()
                        .AnyAsync(p => p.Code != null && p.Code.ToLower() == dto.Code.ToLower());

                    if (codeExists)
                        return ServiceResult<PromotionDto>.Failure(ApiStatusCodes.BadRequest, "Mã voucher đã tồn tại.");
                    promotion.Code = dto.Code;
                }

                if (dto.Name != null) promotion.Name = dto.Name;
                if (dto.Description != null) promotion.Description = dto.Description;
                if (dto.DiscountType != null) promotion.DiscountType = dto.DiscountType;
                if (dto.DiscountValue.HasValue) promotion.DiscountValue = dto.DiscountValue.Value;
                if (dto.MaxDiscount.HasValue) promotion.MaxDiscount = dto.MaxDiscount;
                if (dto.MinOrderValue.HasValue) promotion.MinOrderValue = dto.MinOrderValue;
                if (dto.MaxUsage.HasValue) promotion.MaxUsage = dto.MaxUsage;
                if (dto.StartDate.HasValue) promotion.StartDate = dto.StartDate;
                if (dto.EndDate.HasValue) promotion.EndDate = dto.EndDate;
                if (dto.IsActive.HasValue) promotion.IsActive = dto.IsActive.Value;

                promotion.UpdatedAt = DateTime.UtcNow;
                promotion.UpdateAt = DateTime.UtcNow;

                // Entity đã tracked từ GetByIdAsync, chỉ cần SaveChanges
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<PromotionDto>.Success(_mapper.Map<PromotionDto>(promotion), "Promotion updated.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating promotion {PromotionId}", promotionId);
                return ServiceResult<PromotionDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<bool>> DeleteAsync(Guid promotionId)
        {
            try
            {
                var promotion = await _unitOfWork.PromotionRepository.GetByIdAsync(promotionId);
                if (promotion == null)
                    return ServiceResult<bool>.Failure(ApiStatusCodes.NotFound, "Promotion not found.");

                promotion.IsActive = false;
                promotion.UpdatedAt = DateTime.UtcNow;
                promotion.UpdateAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, "Promotion deactivated.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting promotion {PromotionId}", promotionId);
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        // ============================================
        // Voucher/Discount Methods
        // ============================================

        public async Task<ServiceResult<Promotion>> ValidateVoucherAsync(string code, decimal subTotal)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return ServiceResult<Promotion>.Failure(ApiStatusCodes.BadRequest, "Mã voucher không được để trống");

                var voucher = await _unitOfWork.PromotionRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(p => p.Code != null && p.Code.ToLower() == code.ToLower());

                if (voucher == null)
                    return ServiceResult<Promotion>.Failure(ApiStatusCodes.NotFound, "Mã voucher không tồn tại");

                if (!voucher.IsActive)
                    return ServiceResult<Promotion>.Failure(ApiStatusCodes.BadRequest, "Voucher đã bị vô hiệu hóa");

                // Check usage limit
                if (voucher.MaxUsage.HasValue && voucher.UsedCount >= voucher.MaxUsage.Value)
                    return ServiceResult<Promotion>.Failure(ApiStatusCodes.BadRequest, "Voucher đã hết lượt sử dụng");

                // Check date range
                var now = DateTime.UtcNow;
                if (voucher.StartDate.HasValue && now < voucher.StartDate.Value)
                    return ServiceResult<Promotion>.Failure(ApiStatusCodes.BadRequest, "Voucher chưa đến thời gian sử dụng");

                if (voucher.EndDate.HasValue && now > voucher.EndDate.Value)
                    return ServiceResult<Promotion>.Failure(ApiStatusCodes.BadRequest, "Voucher đã hết hạn");

                // Check min order value
                if (voucher.MinOrderValue.HasValue && subTotal < voucher.MinOrderValue.Value)
                    return ServiceResult<Promotion>.Failure(ApiStatusCodes.BadRequest,
                        $"Đơn hàng tối thiểu {voucher.MinOrderValue.Value:N0}đ để sử dụng voucher này");

                return ServiceResult<Promotion>.Success(voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating voucher {Code}", code);
                return ServiceResult<Promotion>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<DiscountResultDto>> CalculateDiscountAsync(string code, decimal subTotal)
        {
            try
            {
                var voucherResult = await ValidateVoucherAsync(code, subTotal);
                if (!voucherResult.IsSuccess)
                    return ServiceResult<DiscountResultDto>.Failure(voucherResult.StatusCode, voucherResult.Message ?? "Voucher không hợp lệ");

                var voucher = voucherResult.Data!;
                var discountAmount = voucher.CalculateDiscount(subTotal);

                var result = new DiscountResultDto
                {
                    Code = voucher.Code ?? string.Empty,
                    Name = voucher.Name,
                    DiscountType = voucher.DiscountType,
                    DiscountValue = voucher.DiscountValue,
                    DiscountAmount = discountAmount,
                    OriginalSubTotal = subTotal,
                    FinalSubTotal = subTotal - discountAmount
                };

                return ServiceResult<DiscountResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating discount for voucher {Code}", code);
                return ServiceResult<DiscountResultDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<decimal>> ApplyVoucherAsync(string code, decimal subTotal)
        {
            try
            {
                var voucherResult = await ValidateVoucherAsync(code, subTotal);
                if (!voucherResult.IsSuccess)
                    return ServiceResult<decimal>.Failure(voucherResult.StatusCode, voucherResult.Message ?? "Voucher không hợp lệ");

                var voucher = voucherResult.Data!;
                var discountAmount = voucher.CalculateDiscount(subTotal);

                // Increment used count
                voucher.UsedCount++;
                voucher.UpdatedAt = DateTime.UtcNow;
                voucher.UpdateAt = DateTime.UtcNow;

                _unitOfWork.PromotionRepository.Update(voucher);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Voucher {Code} applied. Discount: {Discount}. Used count: {UsedCount}",
                    code, discountAmount, voucher.UsedCount);

                return ServiceResult<decimal>.Success(discountAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying voucher {Code}", code);
                return ServiceResult<decimal>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }
    }
}