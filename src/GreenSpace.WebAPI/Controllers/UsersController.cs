using GreenSpace.Application.DTOs.User;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Constants;
using GreenSpace.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    /// <summary>
    /// User management endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        /// <returns>List of all users</returns>
        /// <response code="200">List of users</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <returns>Current user data</returns>
        /// <response code="200">User profile</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.GetUserId();
            var result = await _userService.GetByIdAsync(userId);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get user by ID (Admin only)
        /// </summary>
        /// <param name="id">User ID (GUID)</param>
        /// <returns>User data</returns>
        /// <response code="200">User found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        /// <response code="404">User not found</response>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _userService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Update user profile (Owner or Admin)
        /// </summary>
        /// <param name="id">User ID to update</param>
        /// <param name="dto">Updated user data</param>
        /// <returns>Updated user</returns>
        /// <response code="200">User updated successfully</response>
        /// <response code="400">Invalid data</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Not owner or admin</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            var currentUserId = User.GetUserId();
            var isAdmin = User.IsInRole(Roles.Admin);

            if (id != currentUserId && !isAdmin)
                return Forbid();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.UpdateAsync(id, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Deactivate user account (Admin only)
        /// </summary>
        /// <param name="id">User ID to deactivate</param>
        /// <returns>Success message</returns>
        /// <response code="200">User deactivated</response>
        /// <response code="400">Error occurred</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin only</response>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _userService.DeactivateAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
