using AutoMapper;
using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.Rating;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services
{
    public class RatingService : IRatingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RatingService> _logger;

        public RatingService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RatingService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IServiceResult<List<RatingDto>>> GetByProductIdAsync(Guid productId)
        {
            try
            {
                var ratings = await _unitOfWork.RatingRepository.GetAllQueryable()
                    .Include(r => r.User)
                    .Include(r => r.Product)
                    .Where(r => r.ProductId == productId)
                    .OrderByDescending(r => r.CreateDate)
                    .ToListAsync();

                var dtos = _mapper.Map<List<RatingDto>>(ratings);
                return ServiceResult<List<RatingDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings for product {ProductId}", productId);
                return ServiceResult<List<RatingDto>>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<List<RatingDto>>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                var ratings = await _unitOfWork.RatingRepository.GetAllQueryable()
                    .Include(r => r.User)
                    .Include(r => r.Product)
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreateDate)
                    .ToListAsync();

                var dtos = _mapper.Map<List<RatingDto>>(ratings);
                return ServiceResult<List<RatingDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings for user {UserId}", userId);
                return ServiceResult<List<RatingDto>>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<RatingDto>> GetByIdAsync(Guid ratingId)
        {
            try
            {
                var rating = await _unitOfWork.RatingRepository.GetAllQueryable()
                    .Include(r => r.User)
                    .Include(r => r.Product)
                    .FirstOrDefaultAsync(r => r.RatingId == ratingId);

                if (rating == null)
                    return ServiceResult<RatingDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Rating.NotFound);

                var dto = _mapper.Map<RatingDto>(rating);
                return ServiceResult<RatingDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rating {RatingId}", ratingId);
                return ServiceResult<RatingDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<RatingDto>> CreateAsync(CreateRatingDto dto, Guid userId)
        {
            try
            {
                // Validate product exists
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(dto.ProductId);
                if (product == null || product.IsActive != true)
                    return ServiceResult<RatingDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Product.NotFound);

                // Check if user already rated this product
                var existingRating = await _unitOfWork.RatingRepository.GetAllQueryable()
                    .AnyAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);

                if (existingRating)
                    return ServiceResult<RatingDto>.Failure(ApiStatusCodes.Conflict, ApiMessages.Rating.Existed);

                var rating = _mapper.Map<Rating>(dto);
                rating.UserId = userId;
                rating.CreateDate = DateTime.UtcNow;

                await _unitOfWork.RatingRepository.AddAsync(rating);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties
                var createdRating = await _unitOfWork.RatingRepository.GetAllQueryable()
                    .Include(r => r.User)
                    .Include(r => r.Product)
                    .FirstOrDefaultAsync(r => r.RatingId == rating.RatingId);

                var result = _mapper.Map<RatingDto>(createdRating);
                return ServiceResult<RatingDto>.Success(result, ApiMessages.Rating.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rating");
                return ServiceResult<RatingDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<RatingDto>> UpdateAsync(
            Guid ratingId,
            UpdateRatingDto dto,
            Guid userId)
        {
            try
            {
                var rating = await _unitOfWork.RatingRepository.GetByIdAsync(ratingId);
                if (rating == null)
                    return ServiceResult<RatingDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Rating.NotFound);

                // Check ownership
                if (rating.UserId != userId)
                    return ServiceResult<RatingDto>.Failure(ApiStatusCodes.Forbidden, ApiMessages.Rating.NoPermit);

                _mapper.Map(dto, rating);

                await _unitOfWork.RatingRepository.UpdateAsync(rating);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties
                var updatedRating = await _unitOfWork.RatingRepository.GetAllQueryable()
                    .Include(r => r.User)
                    .Include(r => r.Product)
                    .FirstOrDefaultAsync(r => r.RatingId == ratingId);

                var result = _mapper.Map<RatingDto>(updatedRating);
                return ServiceResult<RatingDto>.Success(result, ApiMessages.Rating.Updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rating {RatingId}", ratingId);
                return ServiceResult<RatingDto>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<bool>> DeleteAsync(Guid ratingId, Guid userId)
        {
            try
            {
                var rating = await _unitOfWork.RatingRepository.GetByIdAsync(ratingId);
                if (rating == null)
                    return ServiceResult<bool>.Failure(ApiStatusCodes.NotFound, ApiMessages.Rating.NotFound);

                // Check ownership
                if (rating.UserId != userId)
                    return ServiceResult<bool>.Failure(ApiStatusCodes.Forbidden, ApiMessages.Rating.NoPermit);

                await _unitOfWork.RatingRepository.RemoveAsync(rating);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, ApiMessages.Rating.Deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rating {RatingId}", ratingId);
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<decimal>> GetAverageRatingAsync(Guid productId)
        {
            try
            {
                var ratings = await _unitOfWork.RatingRepository.GetAllQueryable()
                    .Where(r => r.ProductId == productId && r.Stars.HasValue)
                    .ToListAsync();

                if (!ratings.Any())
                    return ServiceResult<decimal>.Success(0, ApiMessages.Rating.NoData);

                var average = ratings.Average(r => r.Stars!.Value);
                return ServiceResult<decimal>.Success(average);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average rating for product {ProductId}", productId);
                return ServiceResult<decimal>.Failure(ApiStatusCodes.InternalServerError, $"Error: {ex.Message}");
            }
        }
    }
}