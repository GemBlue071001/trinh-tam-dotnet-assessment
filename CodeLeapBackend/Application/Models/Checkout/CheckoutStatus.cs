namespace Application.Models.Checkout;

public enum CheckoutStatus
{
    Success,
    CartNotFound,
    CartEmpty,
    ProductNotFound,
    InsufficientStock,
    ConcurrencyConflict
}
