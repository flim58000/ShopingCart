using Dapper;
using ShopingCart.DataContract.Models;

namespace ShopingCart.Repository;

public class OrderRepository(DbSession session)
{
    public Order? GetByCartId(string cartId)
    {
        var order = session.Connection.QuerySingleOrDefault<Order>(
            "SELECT Id, CartId, TotalSatang, CreatedAt FROM Orders WHERE CartId = @CartId;",
            new { CartId = cartId }, session.Transaction);
        if (order is null) return null;
        order.Items = session.Connection.Query<OrderItem>("""
            SELECT ProductId, ProductCode, ProductName, UnitPriceSatang, Quantity
            FROM OrderItems WHERE OrderId = @OrderId ORDER BY Id;
            """, new { OrderId = order.Id }, session.Transaction).ToList();
        return order;
    }

    public Order Create(Cart cart)
    {
        var order = new Order
        {
            CartId = cart.Id, TotalSatang = cart.TotalSatang,
            CreatedAt = DateTime.UtcNow.ToString("O")
        };
        order.Id = session.Connection.ExecuteScalar<long>("""
            INSERT INTO Orders (CartId, TotalSatang, CreatedAt) VALUES (@CartId, @TotalSatang, @CreatedAt);
            SELECT last_insert_rowid();
            """, order, session.Transaction);

        foreach (var item in cart.Items)
        {
            session.Connection.Execute("""
                INSERT INTO OrderItems (OrderId, ProductId, ProductCode, ProductName, UnitPriceSatang, Quantity)
                VALUES (@OrderId, @ProductId, @ProductCode, @ProductName, @UnitPriceSatang, @Quantity);
                """, new { OrderId = order.Id, item.ProductId, item.ProductCode, item.ProductName,
                    item.UnitPriceSatang, item.Quantity }, session.Transaction);
        }
        return GetByCartId(cart.Id)!;
    }
}
