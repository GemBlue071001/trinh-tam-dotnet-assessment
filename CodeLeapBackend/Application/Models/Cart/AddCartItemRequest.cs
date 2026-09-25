using System.ComponentModel.DataAnnotations;

namespace Application.Models.Cart;

public sealed class AddCartItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
