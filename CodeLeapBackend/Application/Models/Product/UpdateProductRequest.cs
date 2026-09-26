using System.ComponentModel.DataAnnotations;

namespace Application.Models.Product;

public sealed class UpdateProductRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Required, MaxLength(100)]
    public string Category { get; set; } = null!;

    [Url]
    public string? ImageUrl { get; set; }
}
