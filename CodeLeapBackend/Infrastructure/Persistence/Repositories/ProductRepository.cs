using Application.Models.Product;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(ECommerceDbContext dbContext)
    : GenericRepository<Product>(dbContext), IProductRepository
{
    public async Task<(List<Product> Products, int TotalCount)> Search(SearchProductsRequest request)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(product => !product.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(product => EF.Functions.ILike(product.Name, $"%{keyword}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = request.Category.Trim();
            query = query.Where(product => EF.Functions.ILike(product.Category, $"%{category}%"));
        }

        var totalCount = await query.CountAsync();
        var products = await query
            .OrderByDescending(product => product.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (products, totalCount);
    }
}
