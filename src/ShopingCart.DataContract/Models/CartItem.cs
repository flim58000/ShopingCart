namespace ShopingCart.DataContract.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public long UnitPriceSatang { get; set; }
    public int StockQuantity { get; set; }
    public int Quantity { get; set; }
    public long LineTotalSatang => checked(UnitPriceSatang * Quantity);
}
