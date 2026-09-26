using System.Security.Claims;
using Application.Models.Cart;
using Application.Models.Checkout;
using Application.Services.Cart;
using Application.Services.Checkout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = "Customer")]
public sealed class CartController(ICartService cartService, ICheckoutService checkoutService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CartResponse>> GetCart()
    {
        return Ok(await cartService.GetCart(GetUserId()));
    }

    [HttpPost("items")]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(AddCartItemRequest request)
    {
        var result = await cartService.AddItem(GetUserId(), request);
        return ToActionResult(result);
    }

    [HttpPut("items/{productId:guid}")]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(Guid productId, UpdateCartItemRequest request)
    {
        var result = await cartService.UpdateItem(GetUserId(), productId, request);
        return ToActionResult(result);
    }

    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid productId)
    {
        var result = await cartService.RemoveItem(GetUserId(), productId);
        return ToActionResult(result);
    }

    [HttpPost("checkout")]
    [ProducesResponseType<CheckoutResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Checkout()
    {
        var result = await checkoutService.Checkout(GetUserId());
        return ToCheckoutActionResult(result);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }

    private IActionResult ToActionResult(CartOperationResult result)
    {
        return result.Status switch
        {
            CartOperationStatus.Success => Ok(result.Cart),
            CartOperationStatus.InsufficientStock => BadRequest(new { message = "Requested quantity exceeds available stock." }),
            CartOperationStatus.ProductNotFound => NotFound(new { message = "Product was not found." }),
            CartOperationStatus.CartItemNotFound => NotFound(new { message = "Cart item was not found." }),
            _ => BadRequest()
        };
    }

    private IActionResult ToCheckoutActionResult(CheckoutResult result)
    {
        return result.Status switch
        {
            CheckoutStatus.Success => StatusCode(StatusCodes.Status201Created, result.Order),
            CheckoutStatus.CartNotFound => NotFound(new { message = "Active cart was not found." }),
            CheckoutStatus.CartEmpty => BadRequest(new { message = "Cart is empty." }),
            CheckoutStatus.ProductNotFound => NotFound(new { message = "A product in the cart was not found." }),
            CheckoutStatus.InsufficientStock => BadRequest(new { message = "A product in the cart no longer has enough stock." }),
            CheckoutStatus.ConcurrencyConflict => Conflict(new { message = "Checkout conflicted with another request. Please try again." }),
            _ => BadRequest()
        };
    }
}
