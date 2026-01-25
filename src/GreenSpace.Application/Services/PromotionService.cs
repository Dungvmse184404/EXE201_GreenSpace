using AutoMapper;
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
                    .Where(p => p.IsActive == true && p.StartDate <= now && p.EndDate >= now)
                    .ToListAsync();

                var dtos = _mapper.Map<List<PromotionDto>>(promotions);
                return ServiceResult<List<PromotionDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active promotions");
                return ServiceResult<List<PromotionDto>>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<PromotionDto>> CreateAsync(CreatePromotionDto dto)
        {
            try
            {
                var promotion = _mapper.Map<Promotion>(dto);
                promotion.IsActive = true;
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
                return ServiceResult<PromotionDto>.Failure($"Error: {ex.Message}");
            }
        }
    }
}