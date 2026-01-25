using AutoMapper;
using GreenSpace.Application.Common.Constants;
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
                    .Include(c => c.InverseParent) // This brings the Sub-categories
                    .AsNoTracking()
                    .ToListAsync();

                return ServiceResult<List<CategoryDto>>.Success(_mapper.Map<List<CategoryDto>>(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return ServiceResult<List<CategoryDto>>.Failure(ApiStatusCodes.InternalServerError, "Could not retrieve categories.");
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
                    return ServiceResult<CategoryDto>.Failure(ApiStatusCodes.NotFound, "Category not found.");

                return ServiceResult<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category {CategoryId}", id);
                return ServiceResult<CategoryDto>.Failure(ApiStatusCodes.InternalServerError, "Error retrieving category details.");
            }
        }

        public async Task<IServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto)
        {
            try
            {
                var exists = await _unitOfWork.CategoryRepository.GetAllQueryable()
                    .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());

                if (exists)
                    return ServiceResult<CategoryDto>.Failure(ApiStatusCodes.Conflict, "Category name already exists.");

                var category = _mapper.Map<Category>(dto);

                await _unitOfWork.CategoryRepository.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<CategoryDto>.Success(_mapper.Map<CategoryDto>(category), "Category created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {Name}", dto.Name);
                return ServiceResult<CategoryDto>.Failure(ApiStatusCodes.InternalServerError, "Failed to create category.");
            }
        }

        public async Task<IServiceResult<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto)
        {
            try
            {
                var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
                if (category == null)
                    return ServiceResult<CategoryDto>.Failure(ApiStatusCodes.NotFound, "Category not found.");

                // Business Rule: A category cannot be its own parent
                if (dto.ParentId.HasValue && dto.ParentId.Value == id)
                    return ServiceResult<CategoryDto>.Failure(ApiStatusCodes.BadRequest, "A category cannot be its own parent.");

                _mapper.Map(dto, category);

                await _unitOfWork.CategoryRepository.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<CategoryDto>.Success(_mapper.Map<CategoryDto>(category), "Category updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return ServiceResult<CategoryDto>.Failure(ApiStatusCodes.InternalServerError, "Failed to update category.");
            }
        }

        public async Task<IServiceResult<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var category = await _unitOfWork.CategoryRepository.GetAllQueryable()
                    .Include(c => c.InverseParent)  
                    .Include(c => c.Products) 
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null)
                    return ServiceResult<bool>.Failure(ApiStatusCodes.NotFound, "Category not found.");

                // Business Rule: Prevent deletion if it has children or products
                if (category.InverseParent.Any())
                    return ServiceResult<bool>.Failure(ApiStatusCodes.BadRequest, "Cannot delete category that has sub-categories.");

                if (category.Products != null && category.Products.Any())
                    return ServiceResult<bool>.Failure(ApiStatusCodes.BadRequest, "Cannot delete category that contains products.");

                await _unitOfWork.CategoryRepository.RemoveAsync(category);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, "Category deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return ServiceResult<bool>.Failure(ApiStatusCodes.InternalServerError, "Failed to delete category.");
            }
        }
    }
}