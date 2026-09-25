using Application.Models.Cart;
using CartEntity = Domain.Entities.Cart;

namespace Application.Mappers;

public static class CartMapper
{
    public static CartResponse Map(CartEntity cart)
    {
        var items = cart.Items.Select(item => new CartItemResponse
        {
            ProductId = item.ProductId,
            ProductName = item.Product.Name,
            ImageUrl = item.Product.ImageUrl,
            UnitPrice = item.Product.Price,
            Quantity = item.Quantity,
            LineTotal = item.Product.Price * item.Quantity
        }).ToList();

        return new CartResponse
        {
            CartId = cart.Id,
            Items = items,
            Total = items.Sum(item => item.LineTotal)
        };
    }
}
