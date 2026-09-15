using Dapper;
using Microsoft.Data.Sqlite;
using ShopingCart.BusinessLogic;
using ShopingCart.DataContract.Models;
using ShopingCart.Repository;
using Xunit;

namespace ShopingCart.Tests;

 public class ShoppingTests : IDisposable
{
    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"shopping-test-{Guid.NewGuid()}.db");
    private readonly string connectionString;

    public ShoppingTests()
    {
        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath, ForeignKeys = true, Pooling = false, DefaultTimeout = 15
        }.ToString();
        DatabaseInitializer.Initialize(connectionString);
    }

    private T Run<T>(Func<Scope, T> action)
    {
        using var scope = new Scope(connectionString);
        return action(scope);
    }
    private Cart CreateCart() => Run(s => s.Carts.Create());
    private Cart Add(string id, int product, int quantity = 1) => Run(s => s.Carts.AddItem(id, product, quantity));
    private Cart GetCart(string id) => Run(s => s.Carts.Get(id));
    private Product Product(int id) => Run(s => s.Products.GetById(id)!);
    private long Sql(string sql) => Run(s => s.Session.Connection.ExecuteScalar<long>(sql));
    private Order Buy(string id, long total) => Run(s => s.Checkout.Checkout(id, total));

    [Fact]
    public void Seed_has_four_products_and_restart_does_not_reset_stock()
    {
        Assert.Equal(4, Sql("SELECT COUNT(*) FROM Products"));
        var cart = Add(CreateCart().Id, 1, 2);
        Buy(cart.Id, cart.TotalSatang);
        DatabaseInitializer.Initialize(connectionString);
        Assert.Equal(8, Product(1).StockQuantity);
    }

    [Fact]
    public void Add_merges_same_product_and_does_not_deduct_stock()
    {
        var id = CreateCart().Id;
        Add(id, 1, 2);
        var cart = Add(id, 1, 3);
        Assert.Single(cart.Items);
        Assert.Equal(5, cart.Items[0].Quantity);
        Assert.Equal(99500, cart.TotalSatang);
        Assert.Equal(10, Product(1).StockQuantity);
    }

    [Fact]
    public void Cannot_add_more_than_stock_or_add_sold_out_product()
    {
        var id = CreateCart().Id;
        Add(id, 3, 1);
        Assert.Throws<BusinessException>(() => Add(id, 3, 2));
        Assert.Throws<BusinessException>(() => Add(id, 4));
        Assert.Equal(1, GetCart(id).TotalQuantity);
    }

    [Fact]
    public void Update_to_zero_removes_row_and_does_not_change_stock()
    {
        var cart = Add(CreateCart().Id, 1, 2);
        Run(s => s.Carts.UpdateQuantity(cart.Id, 1, 1));
        var result = Run(s => s.Carts.UpdateQuantity(cart.Id, 1, 0));
        Assert.Empty(result.Items);
        Assert.Equal(0, Sql("SELECT COUNT(*) FROM CartItems"));
        Assert.Equal(10, Product(1).StockQuantity);
    }

    [Fact]
    public void Remove_and_clear_leave_inventory_unchanged()
    {
        var id = CreateCart().Id;
        Add(id, 1, 2);
        Add(id, 2);
        Assert.Single(Run(s => s.Carts.RemoveItem(id, 1)).Items);
        Assert.Empty(Run(s => s.Carts.Clear(id)).Items);
        Assert.Equal(10, Product(1).StockQuantity);
        Assert.Equal(5, Product(2).StockQuantity);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1000001)]
    public void Invalid_add_quantity_is_rejected(int quantity)
    {
        Assert.Throws<BusinessException>(() => Add(CreateCart().Id, 1, quantity));
    }

    [Fact]
    public void Update_cannot_exceed_stock_or_be_negative()
    {
        var cart = Add(CreateCart().Id, 3);
        Assert.Throws<BusinessException>(() => Run(s => s.Carts.UpdateQuantity(cart.Id, 3, 3)));
        Assert.Throws<BusinessException>(() => Run(s => s.Carts.UpdateQuantity(cart.Id, 3, -1)));
        Assert.Equal(1, GetCart(cart.Id).TotalQuantity);
    }

    [Fact]
    public void Checkout_stores_exact_money_clears_cart_and_deducts_stock()
    {
        var id = CreateCart().Id;
        Add(id, 1, 2);
        var cart = Add(id, 3);
        var order = Buy(id, cart.TotalSatang);
        Assert.Equal(128850, order.TotalSatang);
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(8, Product(1).StockQuantity);
        Assert.Equal(1, Product(3).StockQuantity);
        Assert.Empty(GetCart(id).Items);
        Assert.Equal("CheckedOut", GetCart(id).Status);
        Assert.Throws<BusinessException>(() => Add(id, 1));
    }

    [Fact]
    public void Empty_or_missing_cart_cannot_checkout()
    {
        Assert.Throws<BusinessException>(() => Buy(CreateCart().Id, 0));
        Assert.Throws<BusinessException>(() => Buy(Guid.NewGuid().ToString(), 0));
        Assert.Equal(0, Sql("SELECT COUNT(*) FROM Orders"));
    }

    [Fact]
    public void Repeated_checkout_returns_same_order_and_keeps_historical_prices()
    {
        var cart = Add(CreateCart().Id, 1, 2);
        var first = Buy(cart.Id, cart.TotalSatang);
        Sql("UPDATE Products SET Name = 'Changed', PriceSatang = 99900 WHERE Id = 1");
        var repeated = Buy(cart.Id, cart.TotalSatang);
        Assert.Equal(first.Id, repeated.Id);
        Assert.Equal("เมาส์", repeated.Items[0].ProductName);
        Assert.Equal(19900, repeated.Items[0].UnitPriceSatang);
        Assert.Equal(8, Product(1).StockQuantity);
        Assert.Equal(1, Sql("SELECT COUNT(*) FROM Orders"));
    }

    [Fact]
    public void Changed_or_tampered_total_requires_review_before_payment()
    {
        var cart = Add(CreateCart().Id, 1);
        Assert.Throws<BusinessException>(() => Buy(cart.Id, 1));
        Sql("UPDATE Products SET PriceSatang = 29900 WHERE Id = 1");
        Assert.Throws<BusinessException>(() => Buy(cart.Id, cart.TotalSatang));
        Assert.Equal(0, Sql("SELECT COUNT(*) FROM Orders"));
        Assert.Equal(10, Product(1).StockQuantity);
        Assert.Equal(29900, Buy(cart.Id, GetCart(cart.Id).TotalSatang).TotalSatang);
    }

    [Fact]
    public void Stock_change_rejects_entire_cart_and_customer_can_reduce_quantity()
    {
        var first = Add(CreateCart().Id, 3, 2);
        Add(first.Id, 1);
        var second = Add(CreateCart().Id, 3);
        Buy(second.Id, second.TotalSatang);
        Assert.Throws<BusinessException>(() => Buy(first.Id, GetCart(first.Id).TotalSatang));
        Assert.Equal(10, Product(1).StockQuantity);
        Assert.Equal("Open", GetCart(first.Id).Status);
        var reduced = Run(s => s.Carts.UpdateQuantity(first.Id, 3, 1));
        Buy(first.Id, reduced.TotalSatang);
        Assert.Equal(0, Product(3).StockQuantity);
    }

    [Fact]
    public void Failure_during_stock_update_rolls_back_order_and_prior_stock_changes()
    {
        var id = CreateCart().Id;
        Add(id, 1);
        var cart = Add(id, 2);
        Sql("""
            CREATE TRIGGER fail_second_product BEFORE UPDATE OF StockQuantity ON Products
            WHEN OLD.Id = 2 BEGIN SELECT RAISE(ABORT, 'Simulated database failure'); END;
            """);
        Assert.Throws<SqliteException>(() => Buy(id, cart.TotalSatang));
        Assert.Equal(0, Sql("SELECT COUNT(*) FROM Orders"));
        Assert.Equal(0, Sql("SELECT COUNT(*) FROM OrderItems"));
        Assert.Equal(10, Product(1).StockQuantity);
        Assert.Equal(5, Product(2).StockQuantity);
        Assert.Equal(2, GetCart(id).TotalQuantity);
        Assert.Equal("Open", GetCart(id).Status);
    }

    [Fact]
    public async Task Two_buyers_competing_for_last_item_only_one_succeeds()
    {
        Sql("UPDATE Products SET StockQuantity = 1 WHERE Id = 3");
        var first = Add(CreateCart().Id, 3);
        var second = Add(CreateCart().Id, 3);
        using var ready = new Barrier(2);
        Task<Order?> Purchase(Cart cart) => Task.Run(() =>
        {
            ready.SignalAndWait();
            try { return Buy(cart.Id, cart.TotalSatang); }
            catch (BusinessException) { return null; }
        });
        var results = await Task.WhenAll(Purchase(first), Purchase(second));
        Assert.Single(results, result => result is not null);
        Assert.Equal(0, Product(3).StockQuantity);
        Assert.Equal(1, Sql("SELECT COUNT(*) FROM Orders"));
    }

    [Fact]
    public async Task Concurrent_repeated_checkout_only_deducts_once()
    {
        var cart = Add(CreateCart().Id, 3);
        using var ready = new Barrier(2);
        Task<Order> Purchase() => Task.Run(() =>
        {
            ready.SignalAndWait();
            return Buy(cart.Id, cart.TotalSatang);
        });
        var results = await Task.WhenAll(Purchase(), Purchase());
        Assert.Equal(results[0].Id, results[1].Id);
        Assert.Equal(1, Product(3).StockQuantity);
        Assert.Equal(1, Sql("SELECT COUNT(*) FROM Orders"));
    }

    public void Dispose()
    {
        foreach (var suffix in new[] { "", "-wal", "-shm" }) File.Delete(databasePath + suffix);
    }

    private sealed class Scope : IDisposable
    {
        public DbSession Session { get; }
        public CartService Carts { get; }
        public ProductRepository Products { get; }
        public CheckoutService Checkout { get; }
        public Scope(string connectionString)
        {
            Session = new DbSession(connectionString);
            Products = new ProductRepository(Session);
            var carts = new CartRepository(Session);
            Carts = new CartService(Session, carts, Products);
            Checkout = new CheckoutService(Session, carts, Products, new OrderRepository(Session));
        }
        public void Dispose() => Session.Dispose();
    }
}
