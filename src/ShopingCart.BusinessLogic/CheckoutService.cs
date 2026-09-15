using ShopingCart.DataContract.Models;
using ShopingCart.Repository;

namespace ShopingCart.BusinessLogic;

public class CheckoutService(DbSession session, CartRepository carts,
    ProductRepository products, OrderRepository orders)
{
    public Order Checkout(string cartId, long expectedTotalSatang)
    {
        session.BeginTransaction();
        // ส่ง request ซ้ำหลังเน็ตหลุด ให้คืนคำสั่งซื้อเดิม ไม่ตัดสต็อกซ้ำ
        var existingOrder = orders.GetByCartId(cartId);
        if (existingOrder is not null)
        {
            session.Commit();
            return existingOrder;
        }

        var cart = carts.GetById(cartId) ?? throw new BusinessException("ไม่พบตะกร้าสินค้า", 404);
        if (cart.Status != "Open") throw new BusinessException("ตะกร้านี้ปิดแล้ว");
        if (cart.Items.Count == 0) throw new BusinessException("กรุณาเพิ่มสินค้าก่อนชำระเงิน", 400);

        // ยอดจริงมาจากราคาล่าสุดในฐานข้อมูล ไม่ใช้ยอดจากหน้าเว็บเป็นราคาขาย
        if (cart.TotalSatang != expectedTotalSatang)
            throw new BusinessException("ยอดตะกร้ามีการเปลี่ยนแปลง กรุณาตรวจสอบยอดล่าสุดแล้วชำระเงินอีกครั้ง");
        foreach (var item in cart.Items)
        {
            if (item.Quantity > item.StockQuantity)
                throw new BusinessException($"{item.ProductName} เหลือ {item.StockQuantity} ชิ้น กรุณาลดจำนวนในตะกร้า");
        }

        // แบบทดสอบนี้ถือว่าการยืนยันคือชำระเงินจำลองสำเร็จ
        // คำสั่งซื้อ สต็อก และสถานะตะกร้า บันทึกใน transaction เดียวกัน
        var order = orders.Create(cart);
        foreach (var item in cart.Items)
        {
            if (!products.TryDecreaseStock(item.ProductId, item.Quantity))
                throw new BusinessException($"{item.ProductName} มีสต็อกไม่เพียงพอ");
        }
        carts.Clear(cartId);
        carts.MarkCheckedOut(cartId);
        session.Commit();
        return order;
    }
}
