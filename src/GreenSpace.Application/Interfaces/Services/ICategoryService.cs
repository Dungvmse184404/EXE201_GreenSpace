using GreenSpace.Application.DTOs.Category;
using GreenSpace.Domain.Interfaces;


namespace GreenSpace.Application.Interfaces.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Gets all asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IServiceResult<List<CategoryDto>>> GetAllAsync();
        /// <summary>
        /// Gets the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<IServiceResult<CategoryDto>> GetByIdAsync(Guid id);
        /// <summary>
        /// Creates the asynchronous.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        Task<IServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto);
        /// <summary>
        /// Updates the asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        Task<IServiceResult<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto);
        /// <summary>
        /// Deletes the asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<IServiceResult<bool>> DeleteAsync(Guid id);
    }
}
