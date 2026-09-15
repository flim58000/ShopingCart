using System.ComponentModel.DataAnnotations;

namespace ShopingCart.DataContract.Requests;

public class UpdateCartItemRequest
{
    [Required, Range(0, 1_000_000)]
    public int? Quantity { get; set; }
}
