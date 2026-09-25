namespace Application.Models.Cart;

public sealed class CartResponse
{
    public Guid? CartId { get; set; }
    public List<CartItemResponse> Items { get; set; } = [];
    public decimal Total { get; set; }
}
