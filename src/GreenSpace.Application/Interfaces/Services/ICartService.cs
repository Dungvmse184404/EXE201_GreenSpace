using GreenSpace.Application.DTOs.Cart;
using GreenSpace.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface ICartService
    {
        Task<IServiceResult<CartDto>> GetUserCartAsync(Guid userId);
        Task<IServiceResult<CartDto>> AddItemAsync(Guid userId, AddCartItemDto dto);
        Task<IServiceResult<CartDto>> RemoveItemAsync(Guid userId, Guid cartItemId);
        Task<IServiceResult<bool>> ClearCartAsync(Guid userId);
    }
}
