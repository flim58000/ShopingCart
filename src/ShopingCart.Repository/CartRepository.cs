using Dapper;
using ShopingCart.DataContract.Models;

namespace ShopingCart.Repository;

public class CartRepository(DbSession session)
{
    public Cart Create()
    {
        var cart = new Cart { Id = Guid.NewGuid().ToString(), CreatedAt = DateTime.UtcNow.ToString("O") };
        session.Connection.Execute(
            "INSERT INTO Carts (Id, Status, CreatedAt) VALUES (@Id, @Status, @CreatedAt);",
            cart, session.Transaction);
        return cart;
    }

    public Cart? GetById(string cartId)
    {
        var cart = session.Connection.QuerySingleOrDefault<Cart>(
            "SELECT Id, Status, CreatedAt FROM Carts WHERE Id = @CartId;",
            new { CartId = cartId }, session.Transaction);
        if (cart is null) return null;

        const string sql = """
            SELECT p.Id AS ProductId, p.Code AS ProductCode, p.Name AS ProductName,
                   p.PriceSatang AS UnitPriceSatang, p.StockQuantity, ci.Quantity
            FROM CartItems ci JOIN Products p ON p.Id = ci.ProductId
            WHERE ci.CartId = @CartId ORDER BY p.Id;
            """;
        cart.Items = session.Connection.Query<CartItem>(sql, new { CartId = cartId },
            session.Transaction).ToList();
        return cart;
    }

    public void SetQuantity(string cartId, int productId, int quantity)
    {
        const string sql = """
            INSERT INTO CartItems (CartId, ProductId, Quantity) VALUES (@CartId, @ProductId, @Quantity)
            ON CONFLICT (CartId, ProductId) DO UPDATE SET Quantity = excluded.Quantity;
            """;
        session.Connection.Execute(sql, new { CartId = cartId, ProductId = productId, Quantity = quantity },
            session.Transaction);
    }

    public void RemoveItem(string cartId, int productId) => session.Connection.Execute(
        "DELETE FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId;",
        new { CartId = cartId, ProductId = productId }, session.Transaction);

    public void Clear(string cartId) => session.Connection.Execute(
        "DELETE FROM CartItems WHERE CartId = @CartId;", new { CartId = cartId }, session.Transaction);

    public void MarkCheckedOut(string cartId) => session.Connection.Execute(
        "UPDATE Carts SET Status = 'CheckedOut' WHERE Id = @CartId;",
        new { CartId = cartId }, session.Transaction);
}
