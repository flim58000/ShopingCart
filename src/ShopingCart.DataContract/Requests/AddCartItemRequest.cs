using System.ComponentModel.DataAnnotations;

namespace ShopingCart.DataContract.Requests;

public class AddCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 1_000_000)]
    public int Quantity { get; set; } = 1;
}
