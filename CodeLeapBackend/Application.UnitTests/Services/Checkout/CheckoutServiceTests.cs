using Application.Models.Checkout;
using Application.Repositories;
using Application.Services.Checkout;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;
using CartEntity = Domain.Entities.Cart;

namespace Application.UnitTests.Services.Checkout;

public sealed class CheckoutServiceTests
{
    [Fact]
    public async Task Checkout_WhenCartIsValid_CreatesPaidOrderAndDeductsStock()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct(stockQuantity: 5, price: 100m);
        var cart = CreateCart(userId, product, quantity: 2);
        var cartRepository = new Mock<ICartRepository>();
        var orderRepository = new Mock<IOrderRepository>();
        var unitOfWork = CreateUnitOfWork();
        Order? createdOrder = null;
        cartRepository.Setup(repository => repository.GetActiveForCheckout(userId)).ReturnsAsync(cart);
        orderRepository.Setup(repository => repository.Add(It.IsAny<Order>()))
            .Callback<Order>(order => createdOrder = order)
            .Returns(Task.CompletedTask);
        orderRepository.Setup(repository => repository.Save()).ReturnsAsync(1);
        var service = new CheckoutService(
            cartRepository.Object,
            orderRepository.Object,
            unitOfWork.Object);

        var result = await service.Checkout(userId);

        Assert.Equal(CheckoutStatus.Success, result.Status);
        Assert.NotNull(result.Order);
        Assert.Equal(OrderStatus.Paid, result.Order!.Status);
        Assert.Equal(200m, result.Order.TotalAmount);
        Assert.Equal(3, product.StockQuantity);
        Assert.Equal(CartStatus.CheckedOut, cart.Status);
        Assert.NotNull(cart.CheckedOutAt);
        Assert.NotNull(createdOrder);
        Assert.Single(createdOrder!.Items);
        Assert.Equal(100m, createdOrder.Items.Single().UnitPrice);
        orderRepository.Verify(repository => repository.Save(), Times.Once);
    }

    [Fact]
    public async Task Checkout_WhenProductStockIsInsufficient_DoesNotCreateOrderOrChangeCart()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct(stockQuantity: 1, price: 100m);
        var cart = CreateCart(userId, product, quantity: 2);
        var cartRepository = new Mock<ICartRepository>();
        var orderRepository = new Mock<IOrderRepository>();
        var unitOfWork = CreateUnitOfWork();
        cartRepository.Setup(repository => repository.GetActiveForCheckout(userId)).ReturnsAsync(cart);
        var service = new CheckoutService(
            cartRepository.Object,
            orderRepository.Object,
            unitOfWork.Object);

        var result = await service.Checkout(userId);

        Assert.Equal(CheckoutStatus.InsufficientStock, result.Status);
        Assert.Equal(1, product.StockQuantity);
        Assert.Equal(CartStatus.Active, cart.Status);
        Assert.Null(cart.CheckedOutAt);
        orderRepository.Verify(repository => repository.Add(It.IsAny<Order>()), Times.Never);
        orderRepository.Verify(repository => repository.Save(), Times.Never);
    }

    private static Mock<IUnitOfWork> CreateUnitOfWork()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(repository => repository.ExecuteInTransaction(It.IsAny<Func<Task<CheckoutResult>>>() ))
            .Returns((Func<Task<CheckoutResult>> action) => action());

        return unitOfWork;
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

    private static CartEntity CreateCart(Guid userId, Product product, int quantity)
    {
        var cart = new CartEntity
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
