using Application.Models.Cart;

namespace Application.Services.Cart;

public interface ICartService
{
    Task<CartResponse> GetCart(Guid userId);
    Task<CartOperationResult> AddItem(Guid userId, AddCartItemRequest request);
}
