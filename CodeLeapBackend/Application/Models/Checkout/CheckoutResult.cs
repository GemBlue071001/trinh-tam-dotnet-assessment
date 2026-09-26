namespace Application.Models.Checkout;

public sealed class CheckoutResult
{
    public CheckoutStatus Status { get; set; }
    public CheckoutResponse? Order { get; set; }
}
