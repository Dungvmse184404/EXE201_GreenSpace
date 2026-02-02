using GreenSpace.Application.DTOs.Rating;
using GreenSpace.Domain.Interfaces;


namespace GreenSpace.Application.Interfaces.Services
{
    public interface IRatingService
    {
        Task<IServiceResult<List<RatingDto>>> GetByProductIdAsync(Guid productId);
        Task<IServiceResult<List<RatingDto>>> GetByUserIdAsync(Guid userId);
        Task<IServiceResult<RatingDto>> GetByIdAsync(Guid ratingId);
        Task<IServiceResult<RatingDto>> CreateAsync(CreateRatingDto dto, Guid userId);
        Task<IServiceResult<RatingDto>> UpdateAsync(Guid ratingId, UpdateRatingDto dto, Guid userId);
        Task<IServiceResult<bool>> DeleteAsync(Guid ratingId, Guid userId);
        Task<IServiceResult<decimal>> GetAverageRatingAsync(Guid productId);
    }
}
