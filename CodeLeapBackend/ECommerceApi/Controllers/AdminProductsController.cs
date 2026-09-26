using Application.Models.Product;
using Application.Services.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public sealed class AdminProductsController(IProductService productService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        var product = await productService.Create(request);
        return CreatedAtAction(nameof(GetById), new { productId = product.Id }, product);
    }

    [HttpGet("{productId:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid productId)
    {
        var product = await productService.GetById(productId);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPut("{productId:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Update(Guid productId, UpdateProductRequest request)
    {
        var product = await productService.Update(productId, request);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid productId)
    {
        return await productService.Delete(productId) ? NoContent() : NotFound();
    }
}
