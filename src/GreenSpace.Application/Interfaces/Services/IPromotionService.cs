using GreenSpace.Application.DTOs.Promotion;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IPromotionService
    {
        Task<IServiceResult<List<PromotionDto>>> GetActivePromotionsAsync();
        Task<IServiceResult<PromotionDto>> CreateAsync(CreatePromotionDto dto);
    }
}
