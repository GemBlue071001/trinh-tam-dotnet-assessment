namespace Application.Models.Cart;

public sealed class CartOperationResult
{
    public CartOperationStatus Status { get; set; }
    public CartResponse? Cart { get; set; }
}
