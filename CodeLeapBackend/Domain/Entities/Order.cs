using Domain.Enums;

namespace Domain.Entities;

public sealed class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
