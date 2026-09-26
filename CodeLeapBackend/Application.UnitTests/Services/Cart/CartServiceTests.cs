using System.Linq.Expressions;
using Application.Models.Cart;
using Application.Repositories;
using Application.Services.Cart;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.UnitTests.Services.Cart;

public sealed class CartServiceTests
{
    [Fact]
    public async Task UpdateItem_WhenQuantityIsAvailable_UpdatesItemAndSavesCart()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct(stockQuantity: 5, price: 100m);
        var cart = CreateCart(userId, product, quantity: 1);
        var cartRepository = new Mock<ICartRepository>();
        var cartItemRepository = new Mock<ICartItemRepository>();
        var productRepository = new Mock<IGenericRepository<Product>>();
        cartRepository.Setup(repository => repository.GetActiveByUserId(userId)).ReturnsAsync(cart);
        cartRepository.Setup(repository => repository.Save()).ReturnsAsync(1);
        productRepository
            .Setup(repository => repository.GetBy(It.IsAny<Expression<Func<Product, bool>>>() ))
            .ReturnsAsync(product);
        var service = new CartService(cartRepository.Object, cartItemRepository.Object, productRepository.Object);

        var result = await service.UpdateItem(userId, product.Id, new UpdateCartItemRequest { Quantity = 3 });

        Assert.Equal(CartOperationStatus.Success, result.Status);
        Assert.Equal(3, cart.Items.Single().Quantity);
        Assert.NotNull(result.Cart);
        Assert.Equal(300m, result.Cart!.Total);
        cartRepository.Verify(repository => repository.Save(), Times.Once);
    }

    [Fact]
    public async Task RemoveItem_WhenItemExists_DeletesItemAndReturnsCartWithoutItem()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct(stockQuantity: 5, price: 100m);
        var cart = CreateCart(userId, product, quantity: 1);
        var cartRepository = new Mock<ICartRepository>();
        var cartItemRepository = new Mock<ICartItemRepository>();
        var productRepository = new Mock<IGenericRepository<Product>>();
        cartRepository.Setup(repository => repository.GetActiveByUserId(userId)).ReturnsAsync(cart);
        cartRepository.Setup(repository => repository.Save()).ReturnsAsync(1);
        var service = new CartService(cartRepository.Object, cartItemRepository.Object, productRepository.Object);

        var result = await service.RemoveItem(userId, product.Id);

        Assert.Equal(CartOperationStatus.Success, result.Status);
        Assert.Empty(cart.Items);
        Assert.NotNull(result.Cart);
        Assert.Empty(result.Cart!.Items);
        cartItemRepository.Verify(repository => repository.Delete(It.Is<CartItem>(item => item.ProductId == product.Id)), Times.Once);
        cartRepository.Verify(repository => repository.Save(), Times.Once);
    }

    private static Product CreateProduct(int stockQuantity, decimal price)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test product",
            Price = price,
            StockQuantity = stockQuantity,
            Category = "Test"
        };
    }

    private static Domain.Entities.Cart CreateCart(Guid userId, Product product, int quantity)
    {
        var cart = new Domain.Entities.Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = CartStatus.Active
        };

        cart.Items.Add(new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ProductId = product.Id,
            Product = product,
            Quantity = quantity
        });

        return cart;
    }
}
