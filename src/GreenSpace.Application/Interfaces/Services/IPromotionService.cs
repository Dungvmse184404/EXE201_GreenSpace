using GreenSpace.Application.DTOs.Promotion;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IPromotionService
    {
        Task<IServiceResult<List<PromotionDto>>> GetAllAsync();
        Task<IServiceResult<PromotionDto>> GetByIdAsync(Guid promotionId);
        Task<IServiceResult<List<PromotionDto>>> GetActivePromotionsAsync();
        Task<IServiceResult<PromotionDto>> CreateAsync(CreatePromotionDto dto);
        Task<IServiceResult<PromotionDto>> UpdateAsync(Guid promotionId, UpdatePromotionDto dto);
        Task<IServiceResult<bool>> DeleteAsync(Guid promotionId);

        // ============================================
        // Voucher/Discount Methods
        // ============================================

        /// <summary>
        /// Validate voucher code và kiểm tra điều kiện áp dụng
        /// </summary>
        Task<ServiceResult<Promotion>> ValidateVoucherAsync(string code, decimal subTotal);

        /// <summary>
        /// Tính số tiền được giảm (không tăng used_count)
        /// </summary>
        Task<IServiceResult<DiscountResultDto>> CalculateDiscountAsync(string code, decimal subTotal);

        /// <summary>
        /// Áp dụng voucher khi đặt hàng (tăng used_count)
        /// </summary>
        Task<IServiceResult<decimal>> ApplyVoucherAsync(string code, decimal subTotal);
    }
}
