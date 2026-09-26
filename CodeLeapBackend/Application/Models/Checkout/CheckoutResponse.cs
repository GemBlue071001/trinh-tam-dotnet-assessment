using Domain.Enums;

namespace Application.Models.Checkout;

public sealed class CheckoutResponse
{
    public Guid OrderId { get; set; }
    public Guid CartId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public List<CheckoutItemResponse> Items { get; set; } = [];
}
