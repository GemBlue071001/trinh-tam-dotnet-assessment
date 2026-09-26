using Application.Models.Cart;

namespace Application.Services.Cart;

public interface ICartService
{
    Task<CartResponse> GetCart(Guid userId);
    Task<CartOperationResult> AddItem(Guid userId, AddCartItemRequest request);
    Task<CartOperationResult> UpdateItem(Guid userId, Guid productId, UpdateCartItemRequest request);
    Task<CartOperationResult> RemoveItem(Guid userId, Guid productId);
}
