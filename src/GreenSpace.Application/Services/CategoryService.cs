using AutoMapper;
using GreenSpace.Application.DTOs.Category;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IServiceResult<List<CategoryDto>>> GetAllAsync()
        {
            try
            {
                var categories = await _unitOfWork.CategoryRepository.GetAllQueryable()
                    .Include(c => c.Parent)
                    .ToListAsync();

                var dtos = _mapper.Map<List<CategoryDto>>(categories);
                return ServiceResult<List<CategoryDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return ServiceResult<List<CategoryDto>>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<CategoryDto>> GetByIdAsync(Guid id)
        {
            try
            {
                var category = await _unitOfWork.CategoryRepository.GetAllQueryable()
                    .Include(c => c.Parent)
                    .Include(c => c.InverseParent)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null)
                    return ServiceResult<CategoryDto>.Failure("Category not found");

                var dto = _mapper.Map<CategoryDto>(category);
                return ServiceResult<CategoryDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category {CategoryId}", id);
                return ServiceResult<CategoryDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto)
        {
            try
            {
                var category = _mapper.Map<Category>(dto);
                await _unitOfWork.CategoryRepository.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<CategoryDto>(category);
                return ServiceResult<CategoryDto>.Success(result, "Category created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return ServiceResult<CategoryDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto)
        {
            try
            {
                var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
                if (category == null)
                    return ServiceResult<CategoryDto>.Failure("Category not found");

                _mapper.Map(dto, category);
                await _unitOfWork.CategoryRepository.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<CategoryDto>(category);
                return ServiceResult<CategoryDto>.Success(result, "Category updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return ServiceResult<CategoryDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<IServiceResult<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
                if (category == null)
                    return ServiceResult<bool>.Failure("Category not found");

                await _unitOfWork.CategoryRepository.RemoveAsync(category);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, "Category deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return ServiceResult<bool>.Failure($"Error: {ex.Message}");
            }
        }
    }
}