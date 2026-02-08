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