using Application.Models.Product;

namespace Application.Services.Product;

public interface IProductService
{
    Task<ProductResponse> Create(CreateProductRequest request);
    Task<ProductResponse?> GetById(Guid productId);
    Task<PagedProductResponse> Search(SearchProductsRequest request);
    Task<ProductResponse?> Update(Guid productId, UpdateProductRequest request);
    Task<bool> Delete(Guid productId);
}
