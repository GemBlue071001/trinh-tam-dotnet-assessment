using Application.Models.Cart;
using Application.Mappers;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using CartEntity = Domain.Entities.Cart;
using ProductEntity = Domain.Entities.Product;

namespace Application.Services.Cart;

public sealed class CartService(
    ICartRepository cartRepository,
    ICartItemRepository cartItemRepository,
    IProductRepository productRepository) : ICartService
{
    public async Task<CartResponse> GetCart(Guid userId)
    {
        var cart = await cartRepository.GetActiveByUserId(userId);
        return cart is null ? new CartResponse() : CartMapper.Map(cart);
    }

    public async Task<CartOperationResult> AddItem(Guid userId, AddCartItemRequest request)
    {
        var product = await productRepository.GetBy(product =>
            product.Id == request.ProductId && !product.IsDeleted);
        if (product is null)
        {
            return new CartOperationResult { Status = CartOperationStatus.ProductNotFound };
        }

        var cart = await cartRepository.GetActiveByUserId(userId);
        var now = DateTimeOffset.UtcNow;

        if (cart is null)
        {
            cart = new CartEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Status = CartStatus.Active,
                CreatedAt = now,
                UpdatedAt = now
            };

            await cartRepository.Add(cart);
        }

        var item = cart.Items.FirstOrDefault(cartItem => cartItem.ProductId == product.Id);
        if (item is null)
        {
            item = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = product.Id,
                Product = product,
                CreatedAt = now,
                UpdatedAt = now
            };

            cart.Items.Add(item);
        }

        var requestedQuantity = item.Quantity + request.Quantity;
        if (requestedQuantity > product.StockQuantity)
        {
            return new CartOperationResult { Status = CartOperationStatus.InsufficientStock };
        }

        item.Quantity = requestedQuantity;
        item.UpdatedAt = now;
        cart.UpdatedAt = now;
        await cartRepository.Save();

        return new CartOperationResult
        {
            Status = CartOperationStatus.Success,
            Cart = CartMapper.Map(cart)
        };
    }

    public async Task<CartOperationResult> UpdateItem(Guid userId, Guid productId, UpdateCartItemRequest request)
    {
        var cart = await cartRepository.GetActiveByUserId(userId);
        var cartItem = cart?.Items.FirstOrDefault(cartItem => cartItem.ProductId == productId);
        if (cartItem is null)
        {
            return new CartOperationResult { Status = CartOperationStatus.CartItemNotFound };
        }

        var product = await productRepository.GetBy(product =>
            product.Id == productId && !product.IsDeleted);
        if (product is null)
        {
            return new CartOperationResult { Status = CartOperationStatus.ProductNotFound };
        }

        if (request.Quantity > product.StockQuantity)
        {
            return new CartOperationResult { Status = CartOperationStatus.InsufficientStock };
        }

        var now = DateTimeOffset.UtcNow;
        cartItem.Quantity = request.Quantity;
        cartItem.UpdatedAt = now;
        cart!.UpdatedAt = now;
        await cartRepository.Save();

        return new CartOperationResult
        {
            Status = CartOperationStatus.Success,
            Cart = CartMapper.Map(cart)
        };
    }

    public async Task<CartOperationResult> RemoveItem(Guid userId, Guid productId)
    {
        var cart = await cartRepository.GetActiveByUserId(userId);
        var item = cart?.Items.FirstOrDefault(cartItem => cartItem.ProductId == productId);
        if (item is null)
        {
            return new CartOperationResult { Status = CartOperationStatus.CartItemNotFound };
        }

        await cartItemRepository.Delete(item);
        cart!.Items.Remove(item);
        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await cartRepository.Save();

        return new CartOperationResult
        {
            Status = CartOperationStatus.Success,
            Cart = CartMapper.Map(cart)
        };
    }
}
