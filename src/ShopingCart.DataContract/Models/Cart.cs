namespace ShopingCart.DataContract.Models;

public class Cart
{
    public string Id { get; set; } = "";
    public string Status { get; set; } = "Open";
    public string CreatedAt { get; set; } = "";
    public List<CartItem> Items { get; set; } = [];
    public long TotalSatang => Items.Sum(item => item.LineTotalSatang);
    public int TotalQuantity => Items.Sum(item => item.Quantity);
}
