namespace ShopingCart.DataContract.Models;

public class Order
{
    public long Id { get; set; }
    public string CartId { get; set; } = "";
    public long TotalSatang { get; set; }
    public string CreatedAt { get; set; } = "";
    public List<OrderItem> Items { get; set; } = [];
}
