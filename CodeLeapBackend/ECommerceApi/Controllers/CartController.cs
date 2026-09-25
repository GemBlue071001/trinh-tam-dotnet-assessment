using System.Security.Claims;
using Application.Models.Cart;
using Application.Services.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = "Customer")]
public sealed class CartController(ICartService cartService) : ControllerBase
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
            _ => BadRequest()
        };
    }
}
