using Application.Models.Checkout;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services.Checkout;

public sealed class CheckoutService(
    ICartRepository cartRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : ICheckoutService
{
    public Task<CheckoutResult> Checkout(Guid userId)
    {
        return unitOfWork.ExecuteInTransaction(async () =>
        {
            var cart = await cartRepository.GetActiveForCheckout(userId);
            if (cart is null)
            {
                return new CheckoutResult { Status = CheckoutStatus.CartNotFound };
            }

            if (cart.Items.Count == 0)
            {
                return new CheckoutResult { Status = CheckoutStatus.CartEmpty };
            }

            var now = DateTimeOffset.UtcNow;
            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cart.Items)
            {
                var product = cartItem.Product;
                if (product is null || product.IsDeleted)
                {
                    return new CheckoutResult { Status = CheckoutStatus.ProductNotFound };
                }

                if (product.StockQuantity < cartItem.Quantity)
                {
                    return new CheckoutResult { Status = CheckoutStatus.InsufficientStock };
                }

                var lineTotal = product.Price * cartItem.Quantity;
                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    ProductNameSnapshot = product.Name,
                    UnitPrice = product.Price,
                    Quantity = cartItem.Quantity,
                    LineTotal = lineTotal
                });

                product.StockQuantity -= cartItem.Quantity;
                product.UpdatedAt = now;
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CartId = cart.Id,
                Status = OrderStatus.Paid,
                TotalAmount = orderItems.Sum(item => item.LineTotal),
                CreatedAt = now,
                UpdatedAt = now,
                CompletedAt = now,
                Items = orderItems
            };

            cart.Status = CartStatus.CheckedOut;
            cart.CheckedOutAt = now;
            cart.UpdatedAt = now;

            await orderRepository.Add(order);
            await orderRepository.Save();

            return new CheckoutResult
            {
                Status = CheckoutStatus.Success,
                Order = new CheckoutResponse
                {
                    OrderId = order.Id,
                    CartId = cart.Id,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    CompletedAt = order.CompletedAt,
                    Items = order.Items.Select(item => new CheckoutItemResponse
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductNameSnapshot,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        LineTotal = item.LineTotal
                    }).ToList()
                }
            };
        });
    }
}
