using Domain.Enums;

namespace Domain.Entities;

public sealed class Cart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public CartStatus Status { get; set; }
    public DateTimeOffset? CheckedOutAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
