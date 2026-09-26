using Application.Models.Product;
using Domain.Entities;

namespace Application.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<(List<Product> Products, int TotalCount)> Search(SearchProductsRequest request);
}
