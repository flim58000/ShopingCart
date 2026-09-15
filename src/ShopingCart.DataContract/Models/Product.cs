namespace ShopingCart.DataContract.Models;

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    // 199.50 บาท = 19950 สตางค์ คำนวณด้วยจำนวนเต็มเพื่อให้ยอดเงินตรง
    public long PriceSatang { get; set; }
    public int StockQuantity { get; set; }
}
