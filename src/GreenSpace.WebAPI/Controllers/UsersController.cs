using GreenSpace.Application.DTOs.User;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Constants;
using GreenSpace.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GreenSpace.WebAPI.Controllers
{
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

        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.GetUserId();
            var result = await _userService.GetByIdAsync(userId);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _userService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPut("{id:guid}")]
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

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _userService.DeactivateAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}