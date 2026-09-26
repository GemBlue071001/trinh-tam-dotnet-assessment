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
    public async Task GetCart_WhenActiveCartExists_ReturnsMappedCart()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct(stockQuantity: 5, price: 100m);
        var cart = CreateCart(userId, product, quantity: 2);
        var cartRepository = new Mock<ICartRepository>();
        var cartItemRepository = new Mock<ICartItemRepository>();
        var productRepository = new Mock<IGenericRepository<Product>>();
        cartRepository.Setup(repository => repository.GetActiveByUserId(userId)).ReturnsAsync(cart);
        var service = new CartService(cartRepository.Object, cartItemRepository.Object, productRepository.Object);

        var result = await service.GetCart(userId);

        Assert.Equal(cart.Id, result.CartId);
        Assert.Single(result.Items);
        Assert.Equal(product.Id, result.Items.Single().ProductId);
        Assert.Equal(200m, result.Total);
    }

    [Fact]
    public async Task GetCart_WhenNoActiveCartExists_ReturnsEmptyCart()
    {
        var userId = Guid.NewGuid();
        var cartRepository = new Mock<ICartRepository>();
        var cartItemRepository = new Mock<ICartItemRepository>();
        var productRepository = new Mock<IGenericRepository<Product>>();
        cartRepository.Setup(repository => repository.GetActiveByUserId(userId))
            .ReturnsAsync((Domain.Entities.Cart?)null);
        var service = new CartService(cartRepository.Object, cartItemRepository.Object, productRepository.Object);

        var result = await service.GetCart(userId);

        Assert.Null(result.CartId);
        Assert.Empty(result.Items);
        Assert.Equal(0m, result.Total);
    }

    [Fact]
    public async Task AddItem_WhenProductHasAvailableStock_CreatesCartAndAddsItem()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct(stockQuantity: 5, price: 100m);
        var cartRepository = new Mock<ICartRepository>();
        var cartItemRepository = new Mock<ICartItemRepository>();
        var productRepository = new Mock<IGenericRepository<Product>>();
        cartRepository.Setup(repository => repository.GetActiveByUserId(userId))
            .ReturnsAsync((Domain.Entities.Cart?)null);
        cartRepository.Setup(repository => repository.Add(It.IsAny<Domain.Entities.Cart>()))
            .Returns(Task.CompletedTask);
        cartRepository.Setup(repository => repository.Save()).ReturnsAsync(1);
        productRepository
            .Setup(repository => repository.GetBy(It.IsAny<Expression<Func<Product, bool>>>() ))
            .ReturnsAsync(product);
        var service = new CartService(cartRepository.Object, cartItemRepository.Object, productRepository.Object);

        var result = await service.AddItem(userId, new AddCartItemRequest
        {
            ProductId = product.Id,
            Quantity = 2
        });

        Assert.Equal(CartOperationStatus.Success, result.Status);
        Assert.NotNull(result.Cart);
        Assert.Single(result.Cart!.Items);
        Assert.Equal(2, result.Cart.Items.Single().Quantity);
        Assert.Equal(200m, result.Cart.Total);
        cartRepository.Verify(repository => repository.Add(It.Is<Domain.Entities.Cart>(cart =>
            cart.UserId == userId && cart.Items.Single().ProductId == product.Id)), Times.Once);
        cartRepository.Verify(repository => repository.Save(), Times.Once);
    }

    [Fact]
    public async Task AddItem_WhenProductDoesNotExist_ReturnsProductNotFound()
    {
        var userId = Guid.NewGuid();
        var cartRepository = new Mock<ICartRepository>();
        var cartItemRepository = new Mock<ICartItemRepository>();
        var productRepository = new Mock<IGenericRepository<Product>>();
        productRepository
            .Setup(repository => repository.GetBy(It.IsAny<Expression<Func<Product, bool>>>() ))
            .ReturnsAsync((Product?)null);
        var service = new CartService(cartRepository.Object, cartItemRepository.Object, productRepository.Object);

        var result = await service.AddItem(userId, new AddCartItemRequest
        {
            ProductId = Guid.NewGuid(),
            Quantity = 1
        });

        Assert.Equal(CartOperationStatus.ProductNotFound, result.Status);
        cartRepository.Verify(repository => repository.GetActiveByUserId(It.IsAny<Guid>()), Times.Never);
        cartRepository.Verify(repository => repository.Save(), Times.Never);
    }

    [Fact]
    public async Task AddItem_WhenRequestedQuantityExceedsStock_ReturnsInsufficientStockWithoutSaving()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct(stockQuantity: 5, price: 100m);
        var cart = CreateCart(userId, product, quantity: 4);
        var cartRepository = new Mock<ICartRepository>();
        var cartItemRepository = new Mock<ICartItemRepository>();
        var productRepository = new Mock<IGenericRepository<Product>>();
        cartRepository.Setup(repository => repository.GetActiveByUserId(userId)).ReturnsAsync(cart);
        productRepository
            .Setup(repository => repository.GetBy(It.IsAny<Expression<Func<Product, bool>>>() ))
            .ReturnsAsync(product);
        var service = new CartService(cartRepository.Object, cartItemRepository.Object, productRepository.Object);

        var result = await service.AddItem(userId, new AddCartItemRequest
        {
            ProductId = product.Id,
            Quantity = 2
        });

        Assert.Equal(CartOperationStatus.InsufficientStock, result.Status);
        Assert.Equal(4, cart.Items.Single().Quantity);
        cartRepository.Verify(repository => repository.Save(), Times.Never);
    }

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
    public async Task UpdateItem_WhenCartItemDoesNotExist_ReturnsCartItemNotFound()
    {
        var userId = Guid.NewGuid();
        var cartRepository = new Mock<ICartRepository>();
        var cartItemRepository = new Mock<ICartItemRepository>();
        var productRepository = new Mock<IGenericRepository<Product>>();
        cartRepository.Setup(repository => repository.GetActiveByUserId(userId))
            .ReturnsAsync((Domain.Entities.Cart?)null);
        var service = new CartService(cartRepository.Object, cartItemRepository.Object, productRepository.Object);

        var result = await service.UpdateItem(userId, Guid.NewGuid(), new UpdateCartItemRequest { Quantity = 1 });

        Assert.Equal(CartOperationStatus.CartItemNotFound, result.Status);
        productRepository.Verify(repository => repository.GetBy(It.IsAny<Expression<Func<Product, bool>>>()), Times.Never);
        cartRepository.Verify(repository => repository.Save(), Times.Never);
    }

    [Fact]
    public async Task UpdateItem_WhenProductWasDeleted_ReturnsProductNotFoundWithoutSaving()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct(stockQuantity: 5, price: 100m);
        var cart = CreateCart(userId, product, quantity: 1);
        var cartRepository = new Mock<ICartRepository>();
        var cartItemRepository = new Mock<ICartItemRepository>();
        var productRepository = new Mock<IGenericRepository<Product>>();
        cartRepository.Setup(repository => repository.GetActiveByUserId(userId)).ReturnsAsync(cart);
        productRepository
            .Setup(repository => repository.GetBy(It.IsAny<Expression<Func<Product, bool>>>() ))
            .ReturnsAsync((Product?)null);
        var service = new CartService(cartRepository.Object, cartItemRepository.Object, productRepository.Object);

        var result = await service.UpdateItem(userId, product.Id, new UpdateCartItemRequest { Quantity = 2 });

        Assert.Equal(CartOperationStatus.ProductNotFound, result.Status);
        Assert.Equal(1, cart.Items.Single().Quantity);
        cartRepository.Verify(repository => repository.Save(), Times.Never);
    }

    [Fact]
    public async Task UpdateItem_WhenRequestedQuantityExceedsStock_ReturnsInsufficientStockWithoutSaving()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct(stockQuantity: 5, price: 100m);
        var cart = CreateCart(userId, product, quantity: 1);
        var cartRepository = new Mock<ICartRepository>();
        var cartItemRepository = new Mock<ICartItemRepository>();
        var productRepository = new Mock<IGenericRepository<Product>>();
        cartRepository.Setup(repository => repository.GetActiveByUserId(userId)).ReturnsAsync(cart);
        productRepository
            .Setup(repository => repository.GetBy(It.IsAny<Expression<Func<Product, bool>>>() ))
            .ReturnsAsync(product);
        var service = new CartService(cartRepository.Object, cartItemRepository.Object, productRepository.Object);

        var result = await service.UpdateItem(userId, product.Id, new UpdateCartItemRequest { Quantity = 6 });

        Assert.Equal(CartOperationStatus.InsufficientStock, result.Status);
        Assert.Equal(1, cart.Items.Single().Quantity);
        cartRepository.Verify(repository => repository.Save(), Times.Never);
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

    [Fact]
    public async Task RemoveItem_WhenCartItemDoesNotExist_ReturnsCartItemNotFound()
    {
        var userId = Guid.NewGuid();
        var cartRepository = new Mock<ICartRepository>();
        var cartItemRepository = new Mock<ICartItemRepository>();
        var productRepository = new Mock<IGenericRepository<Product>>();
        cartRepository.Setup(repository => repository.GetActiveByUserId(userId))
            .ReturnsAsync((Domain.Entities.Cart?)null);
        var service = new CartService(cartRepository.Object, cartItemRepository.Object, productRepository.Object);

        var result = await service.RemoveItem(userId, Guid.NewGuid());

        Assert.Equal(CartOperationStatus.CartItemNotFound, result.Status);
        cartItemRepository.Verify(repository => repository.Delete(It.IsAny<CartItem>()), Times.Never);
        cartRepository.Verify(repository => repository.Save(), Times.Never);
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
