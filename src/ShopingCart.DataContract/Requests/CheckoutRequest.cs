using System.ComponentModel.DataAnnotations;

namespace ShopingCart.DataContract.Requests;

public class CheckoutRequest
{
    // ส่งยอดที่ผู้ซื้อเห็นมาเปรียบเทียบเท่านั้น ยอดที่บันทึกต้องคำนวณจากฐานข้อมูล
    [Required, Range(typeof(long), "0", "9007199254740991")]
    public long? ExpectedTotalSatang { get; set; }
}
