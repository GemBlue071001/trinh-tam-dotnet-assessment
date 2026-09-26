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
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(await cartService.GetCart(userId));
    }

    [HttpPost("items")]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(AddCartItemRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await cartService.AddItem(userId, request);
        return ToActionResult(result);
    }

    [HttpPut("items/{productId:guid}")]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(Guid productId, UpdateCartItemRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await cartService.UpdateItem(userId, productId, request);
        return ToActionResult(result);
    }

    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid productId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await cartService.RemoveItem(userId, productId);
        return ToActionResult(result);
    }

    [HttpPost("checkout")]
    [ProducesResponseType<CheckoutResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Checkout()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await checkoutService.Checkout(userId);
        return ToCheckoutActionResult(result);
    }

    private bool TryGetUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
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
