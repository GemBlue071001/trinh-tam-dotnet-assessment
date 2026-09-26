using System.ComponentModel.DataAnnotations;

namespace Application.Models.Product;

public sealed class SearchProductsRequest
{
    public string? Keyword { get; set; }
    public string? Category { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
