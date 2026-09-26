namespace Application.Models.Product;

public sealed class PagedProductResponse
{
    public List<ProductResponse> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
