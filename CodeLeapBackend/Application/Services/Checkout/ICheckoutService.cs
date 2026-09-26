using Application.Models.Checkout;

namespace Application.Services.Checkout;

public interface ICheckoutService
{
    Task<CheckoutResult> Checkout(Guid userId);
}
