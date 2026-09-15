using Dapper;
using ShopingCart.DataContract.Models;

namespace ShopingCart.Repository;

public class ProductRepository(DbSession session)
{
    public List<Product> GetAll() => session.Connection.Query<Product>(
        "SELECT Id, Code, Name, PriceSatang, StockQuantity FROM Products ORDER BY Id;",
        transaction: session.Transaction).ToList();

    public Product? GetById(int id) => session.Connection.QuerySingleOrDefault<Product>(
        "SELECT Id, Code, Name, PriceSatang, StockQuantity FROM Products WHERE Id = @Id;",
        new { Id = id }, session.Transaction);

    public bool TryDecreaseStock(int productId, int quantity)
    {
        const string sql = """
            UPDATE Products SET StockQuantity = StockQuantity - @Quantity
            WHERE Id = @ProductId AND StockQuantity >= @Quantity;
            """;
        return session.Connection.Execute(sql, new { ProductId = productId, Quantity = quantity },
            session.Transaction) == 1;
    }
}
