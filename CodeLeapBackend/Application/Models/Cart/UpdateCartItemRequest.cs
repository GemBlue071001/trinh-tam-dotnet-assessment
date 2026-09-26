using System.ComponentModel.DataAnnotations;

namespace Application.Models.Cart;

public sealed class UpdateCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
