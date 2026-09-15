using ShopingCart.DataContract.Models;
using ShopingCart.Repository;

namespace ShopingCart.BusinessLogic;

public class CartService(DbSession session, CartRepository carts, ProductRepository products)
{
    public Cart Create() => carts.Create();

    public Cart Get(string cartId) => carts.GetById(cartId)
        ?? throw new BusinessException("ไม่พบตะกร้าสินค้า", 404);

    public Cart AddItem(string cartId, int productId, int quantity)
    {
        ValidateQuantity(quantity, allowZero: false);
        session.BeginTransaction();
        var cart = GetOpenCart(cartId);
        var product = products.GetById(productId)
            ?? throw new BusinessException("ไม่พบสินค้า", 404);
        var currentQuantity = cart.Items.Find(item => item.ProductId == productId)?.Quantity ?? 0;
        var newQuantity = currentQuantity + quantity;
        if (newQuantity > product.StockQuantity)
            throw new BusinessException($"{product.Name} มีสินค้าเหลือ {product.StockQuantity} ชิ้น เพิ่มเกินสต็อกไม่ได้");

        carts.SetQuantity(cartId, productId, newQuantity);
        var result = Get(cartId);
        session.Commit();
        return result;
    }

    public Cart UpdateQuantity(string cartId, int productId, int quantity)
    {
        ValidateQuantity(quantity, allowZero: true);
        session.BeginTransaction();
        var cart = GetOpenCart(cartId);
        var item = cart.Items.Find(item => item.ProductId == productId)
            ?? throw new BusinessException("ไม่พบสินค้านี้ในตะกร้า", 404);

        // ถ้าสต็อกลดลงระหว่างซื้อ ผู้ซื้อยังลดจำนวนหรือลบรายการเดิมได้
        if (quantity > item.Quantity && quantity > item.StockQuantity)
            throw new BusinessException($"{item.ProductName} มีสินค้าเหลือ {item.StockQuantity} ชิ้น");
        if (quantity == 0) carts.RemoveItem(cartId, productId);
        else carts.SetQuantity(cartId, productId, quantity);

        var result = Get(cartId);
        session.Commit();
        return result;
    }

    public Cart RemoveItem(string cartId, int productId)
    {
        session.BeginTransaction();
        GetOpenCart(cartId);
        carts.RemoveItem(cartId, productId);
        var result = Get(cartId);
        session.Commit();
        return result;
    }

    public Cart Clear(string cartId)
    {
        session.BeginTransaction();
        GetOpenCart(cartId);
        carts.Clear(cartId);
        var result = Get(cartId);
        session.Commit();
        return result;
    }

    private Cart GetOpenCart(string cartId)
    {
        var cart = Get(cartId);
        if (cart.Status != "Open")
            throw new BusinessException("ตะกร้านี้ชำระเงินแล้ว กรุณาเริ่มตะกร้าใหม่");
        return cart;
    }

    private static void ValidateQuantity(int quantity, bool allowZero)
    {
        if (quantity < (allowZero ? 0 : 1) || quantity > 1_000_000)
            throw new BusinessException("จำนวนสินค้าไม่ถูกต้อง", 400);
    }
}
