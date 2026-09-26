using Application.Models.Product;
using Application.Repositories;
using Domain.Entities;

namespace Application.Services.Product;

public sealed class ProductService(IGenericRepository<Domain.Entities.Product> productRepository) : IProductService
{
    public async Task<ProductResponse> Create(CreateProductRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        var product = new Domain.Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Category = request.Category.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await productRepository.Add(product);
        await productRepository.Save();

        return Map(product);
    }

    public async Task<ProductResponse?> GetById(Guid productId)
    {
        var product = await GetActiveProduct(productId);
        return product is null ? null : Map(product);
    }

    public async Task<ProductResponse?> Update(Guid productId, UpdateProductRequest request)
    {
        var product = await GetActiveProduct(productId);
        if (product is null)
        {
            return null;
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.Category = request.Category.Trim();
        product.ImageUrl = request.ImageUrl?.Trim();
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await productRepository.Update(product);
        await productRepository.Save();

        return Map(product);
    }

    public async Task<bool> Delete(Guid productId)
    {
        var product = await GetActiveProduct(productId);
        if (product is null)
        {
            return false;
        }

        product.IsDeleted = true;
        product.DeletedAt = DateTimeOffset.UtcNow;
        product.UpdatedAt = product.DeletedAt.Value;

        await productRepository.Update(product);
        await productRepository.Save();

        return true;
    }

    private Task<Domain.Entities.Product?> GetActiveProduct(Guid productId)
    {
        return productRepository.GetBy(product => product.Id == productId && !product.IsDeleted);
    }

    private static ProductResponse Map(Domain.Entities.Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
