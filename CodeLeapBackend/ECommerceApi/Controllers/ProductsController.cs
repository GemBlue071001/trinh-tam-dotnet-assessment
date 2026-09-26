using Application.Models.Product;
using Application.Services.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/products")]
[AllowAnonymous]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedProductResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedProductResponse>> Search([FromQuery] SearchProductsRequest request)
    {
        return Ok(await productService.Search(request));
    }

    [HttpGet("{productId:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid productId)
    {
        var product = await productService.GetById(productId);
        return product is null ? NotFound() : Ok(product);
    }
}
