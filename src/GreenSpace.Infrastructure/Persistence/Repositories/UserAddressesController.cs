using GreenSpace.Application.DTOs.User;
using GreenSpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GreenSpace.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserAddressesController : ControllerBase
    {
        private readonly IUserAddressService _addressService;

        public UserAddressesController(IUserAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet("my-addresses")]
        public async Task<IActionResult> GetMyAddresses()
        {
            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _addressService.GetByUserIdAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _addressService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserAddressDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _addressService.CreateAsync(dto, userId);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Data?.AddressId }, result)
                : BadRequest(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserAddressDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _addressService.UpdateAsync(id, dto, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue("uid")!);
            var result = await _addressService.DeleteAsync(id, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}