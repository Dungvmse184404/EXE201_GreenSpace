using GreenSpace.Application.DTOs.UserAddress;
using GreenSpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GreenSpace.WebAPI.Controllers;

/// <summary>
/// User address management endpoints
/// </summary>
[ApiController]
[Route("api/users/me/addresses")]
[Authorize]
public class UserAddressesController : ControllerBase
{
    private readonly IUserAddressService _addressService;

    public UserAddressesController(IUserAddressService addressService)
    {
        _addressService = addressService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue("uid")!);

    /// <summary>
    /// Get all addresses of current user
    /// </summary>
    /// <returns>List of user addresses</returns>
    /// <response code="200">List of addresses</response>
    /// <response code="400">Error occurred</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserAddressResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyAddresses()
    {
        var result = await _addressService.GetByUserIdAsync(GetUserId());
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get default address of current user
    /// </summary>
    /// <returns>Default address</returns>
    /// <response code="200">Default address found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">No default address set</response>
    [HttpGet("default")]
    [ProducesResponseType(typeof(UserAddressResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefaultAddress()
    {
        var result = await _addressService.GetDefaultAsync(GetUserId());
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get address by ID
    /// </summary>
    /// <param name="addressId">Address ID (GUID)</param>
    /// <returns>Address data</returns>
    /// <response code="200">Address found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Address not found or not owned by user</response>
    [HttpGet("{addressId:guid}")]
    [ProducesResponseType(typeof(UserAddressResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid addressId)
    {
        var result = await _addressService.GetByIdAsync(addressId, GetUserId());
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new address
    /// </summary>
    /// <param name="dto">Address data</param>
    /// <returns>Created address</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/users/me/addresses
    ///     {
    ///         "province": "TP. Ho Chi Minh",      // Tinh/Thanh pho
    ///         "district": "Quan 1",               // Quan/Huyen
    ///         "ward": "Phuong Ben Nghe",          // Phuong/Xa
    ///         "streetAddress": "123 Nguyen Hue",  // So nha, ten duong
    ///         "label": "Nha",                     // Nhan: Nha, Cong ty, etc.
    ///         "isDefault": true                   // Dat lam dia chi mac dinh
    ///     }
    /// </remarks>
    /// <response code="201">Address created successfully</response>
    /// <response code="400">Invalid data</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserAddressResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateUserAddressDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _addressService.CreateAsync(dto, GetUserId());
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { addressId = ((dynamic)result).Data?.AddressId }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update an address
    /// </summary>
    /// <param name="addressId">Address ID to update</param>
    /// <param name="dto">Updated address data</param>
    /// <returns>Updated address</returns>
    /// <response code="200">Address updated successfully</response>
    /// <response code="400">Invalid data or address not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("{addressId:guid}")]
    [ProducesResponseType(typeof(UserAddressResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid addressId, [FromBody] UpdateUserAddressDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _addressService.UpdateAsync(addressId, dto, GetUserId());
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete an address
    /// </summary>
    /// <param name="addressId">Address ID to delete</param>
    /// <returns>Success message</returns>
    /// <response code="200">Address deleted successfully</response>
    /// <response code="400">Address not found or cannot delete</response>
    /// <response code="401">Unauthorized</response>
    [HttpDelete("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid addressId)
    {
        var result = await _addressService.DeleteAsync(addressId, GetUserId());
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Set address as default
    /// </summary>
    /// <param name="addressId">Address ID to set as default</param>
    /// <returns>Updated address</returns>
    /// <response code="200">Address set as default</response>
    /// <response code="400">Address not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("{addressId:guid}/default")]
    [ProducesResponseType(typeof(UserAddressResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetDefault(Guid addressId)
    {
        var result = await _addressService.SetDefaultAsync(addressId, GetUserId());
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
